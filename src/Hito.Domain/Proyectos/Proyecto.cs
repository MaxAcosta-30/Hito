using Hito.Domain.Comun;

namespace Hito.Domain.Proyectos;

// Raíz del agregado: toda regla sobre versiones e hitos pasa por aquí.
public sealed class Proyecto : IPerteneceAUsuario
{
    public const int MaximoNombre = 120;

    private readonly List<VersionDeProyecto> _versiones = [];
    private readonly List<HitoDeProyecto> _hitos = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public Guid ClienteId { get; private set; }
    public string Nombre { get; private set; } = null!;
    public EstadoProyecto Estado { get; private set; } = EstadoProyecto.Borrador;
    public DateTimeOffset CreadoEn { get; private set; }
    // Cambia con cada operación: así la fila siempre se actualiza y el token de concurrencia (xmin) detecta choques.
    public DateTimeOffset ActualizadoEn { get; private set; }
    public uint VersionDeFila { get; private set; }

    public IReadOnlyList<VersionDeProyecto> Versiones => _versiones;
    public IReadOnlyList<HitoDeProyecto> Hitos => _hitos;

    public VersionDeProyecto UltimaVersion => _versiones.MaxBy(v => v.Numero)!;

    // La última versión aceptada: son los términos que rigen el trabajo y el cobro.
    public VersionDeProyecto? VersionVigente =>
        _versiones.Where(v => v.Estado == EstadoVersion.Aceptada).MaxBy(v => v.Numero);

    // Lo que se muestra como alcance actual: la versión vigente o, si nunca se ha aceptado, la última.
    public VersionDeProyecto VersionActual => VersionVigente ?? UltimaVersion;

    public decimal Total => VersionActual.Total;

    private Proyecto() { }

    public static Proyecto Crear(
        Guid ownerId, Guid clienteId, string nombre, IReadOnlyList<DatosDeHito> hitos, DateTimeOffset ahora)
    {
        var proyecto = new Proyecto { OwnerId = ownerId, ClienteId = clienteId, CreadoEn = ahora };
        proyecto._versiones.Add(new VersionDeProyecto(ownerId, proyecto.Id, 1, ahora));
        proyecto.DefinirAlcance(nombre, hitos, ahora);
        return proyecto;
    }

    // Reemplaza el nombre y los hitos de la versión en borrador. Los hitos con Id se actualizan, los nuevos se crean
    // y los que faltan se quitan. Un hito que ya avanzó (entregado, aprobado, pagado) no puede cambiar ni quitarse.
    public void DefinirAlcance(string nombre, IReadOnlyList<DatosDeHito> hitos, DateTimeOffset ahora)
    {
        ExigirQueSigaAbierto();
        var borrador = UltimaVersion;
        if (borrador.Estado != EstadoVersion.Borrador)
            throw new ReglaDeNegocioException("Esta versión ya se envió al cliente. Crea una nueva versión para cambiar el alcance.");
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > MaximoNombre)
            throw new ArgumentException("Nombre de proyecto inválido.", nameof(nombre));
        if (hitos.Count is 0 or > DatosDeHito.MaximoHitos)
            throw new ReglaDeNegocioException($"El proyecto debe tener entre 1 y {DatosDeHito.MaximoHitos} hitos.");
        if (hitos.Any(h => h.Errores().Any()))
            throw new ArgumentException("Hay hitos con datos inválidos.", nameof(hitos));
        var ids = hitos.Where(h => h.HitoId is not null).Select(h => h.HitoId!.Value).ToList();
        if (ids.Count != ids.Distinct().Count())
            throw new ReglaDeNegocioException("Un hito aparece dos veces.");

        var enAlcance = new HashSet<Guid>();
        foreach (var (datos, orden) in hitos.Select((h, i) => (h, i)))
        {
            if (datos.HitoId is null)
            {
                var nuevo = new HitoDeProyecto(OwnerId, Id);
                _hitos.Add(nuevo);
                borrador.AgregarTerminos(nuevo.Id, orden, datos);
                enAlcance.Add(nuevo.Id);
                continue;
            }

            var hito = _hitos.FirstOrDefault(h => h.Id == datos.HitoId)
                ?? throw new ReglaDeNegocioException("Uno de los hitos no pertenece a este proyecto.");
            var vigentes = VersionVigente?.TerminosDe(hito.Id);
            if (hito.Estado != EstadoHito.Pendiente && (vigentes is null || !vigentes.MismosTerminos(datos)))
                throw new ReglaDeNegocioException($"«{vigentes?.Titulo ?? datos.Titulo}» ya avanzó; sus términos no pueden cambiar.");

            if (borrador.TerminosDe(hito.Id) is { } existentes) existentes.Actualizar(orden, datos);
            else borrador.AgregarTerminos(hito.Id, orden, datos);
            enAlcance.Add(hito.Id);
        }

        foreach (var quitado in borrador.Terminos.Where(t => !enAlcance.Contains(t.HitoId)).ToList())
        {
            var hito = _hitos.First(h => h.Id == quitado.HitoId);
            if (hito.Estado != EstadoHito.Pendiente)
                throw new ReglaDeNegocioException($"«{quitado.Titulo}» ya avanzó; no se puede quitar del alcance.");
            borrador.QuitarTerminos(quitado);
            // Si ninguna otra versión lo menciona, el hito nunca existió para el cliente y se borra.
            if (_versiones.All(v => v.TerminosDe(hito.Id) is null)) _hitos.Remove(hito);
        }

        Nombre = nombre.Trim();
        ActualizadoEn = ahora;
    }

    // Un proyecto con pagos se puede cancelar: se conserva con todo su historial.
    public void Cancelar(DateTimeOffset ahora)
    {
        ExigirQueSigaAbierto();
        Estado = EstadoProyecto.Cancelado;
        ActualizadoEn = ahora;
    }

    private void ExigirQueSigaAbierto()
    {
        if (Estado is EstadoProyecto.Cancelado or EstadoProyecto.Completado)
            throw new ReglaDeNegocioException(Estado == EstadoProyecto.Cancelado
                ? "El proyecto está cancelado."
                : "El proyecto ya está completado.");
    }
}
