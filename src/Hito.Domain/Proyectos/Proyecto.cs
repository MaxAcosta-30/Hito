using System.Globalization;
using Hito.Domain.Comun;

namespace Hito.Domain.Proyectos;

// Raíz del agregado: toda regla sobre versiones e hitos pasa por aquí.
public sealed class Proyecto : IPerteneceAUsuario
{
    public const int MaximoNombre = 120;

    private readonly List<VersionDeProyecto> _versiones = [];
    private readonly List<HitoDeProyecto> _hitos = [];
    private readonly List<Pago> _pagos = [];
    private readonly List<EventoDeProyecto> _eventosNuevos = [];

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
    public IReadOnlyList<Pago> Pagos => _pagos;

    // Eventos generados en esta operación y aún sin guardar. HitoDbContext los guarda al llamar SaveChanges.
    public IReadOnlyList<EventoDeProyecto> EventosNuevos => _eventosNuevos;

    public void OlvidarEventosNuevos() => _eventosNuevos.Clear();

    private void Registrar(Actor actor, TipoDeEvento tipo, string descripcion, DateTimeOffset ahora) =>
        _eventosNuevos.Add(new EventoDeProyecto(OwnerId, Id, actor, tipo, descripcion, ahora));

    private string TituloDe(Guid hitoId) =>
        (VersionVigente ?? UltimaVersion).TerminosDe(hitoId)?.Titulo ?? "un hito";

    public VersionDeProyecto UltimaVersion => _versiones.MaxBy(v => v.Numero)!;

    // La última versión aceptada: son los términos que rigen el trabajo y el cobro.
    public VersionDeProyecto? VersionVigente =>
        _versiones.Where(v => v.Estado == EstadoVersion.Aceptada).MaxBy(v => v.Numero);

    // Lo que se muestra como alcance actual: la versión vigente o, si nunca se ha aceptado, la última.
    public VersionDeProyecto VersionActual => VersionVigente ?? UltimaVersion;

    public decimal Total => VersionActual.Total;

    public decimal PagadoDe(Guid hitoId) => _pagos.Where(p => p.HitoId == hitoId).Sum(p => p.Monto);

    public decimal Pagado => _pagos.Sum(p => p.Monto);

    private Proyecto() { }

    public static Proyecto Crear(
        Guid ownerId, Guid clienteId, string nombre, IReadOnlyList<DatosDeHito> hitos, DateTimeOffset ahora)
    {
        var proyecto = new Proyecto { OwnerId = ownerId, ClienteId = clienteId, CreadoEn = ahora };
        proyecto._versiones.Add(new VersionDeProyecto(ownerId, proyecto.Id, 1, ahora));
        proyecto.DefinirAlcance(nombre, hitos, ahora);
        proyecto._eventosNuevos.Clear();
        proyecto.Registrar(Actor.Freelancer, TipoDeEvento.Creado,
            $"creó el proyecto con {hitos.Count} {(hitos.Count == 1 ? "hito" : "hitos")} por {EventoDeProyecto.Dinero(proyecto.Total)}.", ahora);
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
        Registrar(Actor.Freelancer, TipoDeEvento.AlcanceEditado,
            $"editó el alcance de la versión {borrador.Numero}: {borrador.Terminos.Count} hitos por {EventoDeProyecto.Dinero(borrador.Total)}.", ahora);
    }

    // Manda la versión en borrador al cliente. El primer envío pasa el proyecto de Borrador a Enviado.
    public void Enviar(DateTimeOffset ahora)
    {
        ExigirQueSigaAbierto();
        var borrador = UltimaVersion;
        if (borrador.Estado != EstadoVersion.Borrador)
            throw new ReglaDeNegocioException("Esta versión ya se envió. Genera un enlace nuevo si el cliente lo perdió.");
        borrador.Enviar(ahora);
        if (Estado == EstadoProyecto.Borrador) Estado = EstadoProyecto.Enviado;
        ActualizadoEn = ahora;
        Registrar(Actor.Freelancer, TipoDeEvento.VersionEnviada,
            $"envió la versión {borrador.Numero} al cliente por {EventoDeProyecto.Dinero(borrador.Total)}.", ahora);
    }

    // El cliente acepta una versión concreta: si mientras tanto el freelancer creó otra, la aceptación se rechaza
    // y nadie acepta algo distinto de lo que vio. Al aceptarse, sus términos quedan congelados.
    public void Aceptar(int numeroDeVersion, DateTimeOffset ahora)
    {
        ExigirQueSigaAbierto();
        var ultima = UltimaVersion;
        if (ultima.Estado != EstadoVersion.Enviada || ultima.Numero != numeroDeVersion)
            throw new ReglaDeNegocioException("Esta versión ya no está disponible para aceptar. Recarga la página.");

        // Un hito pudo avanzar mientras esta versión esperaba respuesta: sus términos deben seguir iguales.
        var vigente = VersionVigente;
        foreach (var hito in _hitos.Where(h => h.Estado != EstadoHito.Pendiente))
        {
            var antes = vigente?.TerminosDe(hito.Id);
            var despues = ultima.TerminosDe(hito.Id);
            if (antes is not null && (despues is null || !despues.MismosTerminos(antes)))
                throw new ReglaDeNegocioException($"«{antes.Titulo}» ya avanzó y esta versión lo cambia. Pide una versión nueva.");
        }

        ultima.Aceptar(ahora);
        if (Estado == EstadoProyecto.Enviado) Estado = EstadoProyecto.Aceptado;
        ActualizadoEn = ahora;
        Registrar(Actor.Cliente, TipoDeEvento.VersionAceptada,
            $"aceptó la versión {ultima.Numero} del alcance por {EventoDeProyecto.Dinero(ultima.Total)}.", ahora);
    }

    // Un cambio después de enviar crea una versión nueva en borrador, copiando la última.
    // La versión vigente sigue rigiendo hasta que el cliente acepte la nueva.
    public VersionDeProyecto NuevaVersion(DateTimeOffset ahora)
    {
        ExigirQueSigaAbierto();
        var ultima = UltimaVersion;
        if (ultima.Estado == EstadoVersion.Borrador)
            throw new ReglaDeNegocioException("Ya hay una versión en borrador; edítala.");
        if (ultima.Estado == EstadoVersion.Enviada) ultima.Reemplazar();

        var nueva = new VersionDeProyecto(OwnerId, Id, ultima.Numero + 1, ahora);
        foreach (var t in ultima.Terminos.OrderBy(t => t.Orden))
            nueva.AgregarTerminos(t.HitoId, t.Orden, new DatosDeHito(t.HitoId, t.Titulo, t.Monto, t.FechaEntrega, t.Entregable));
        _versiones.Add(nueva);
        ActualizadoEn = ahora;
        Registrar(Actor.Freelancer, TipoDeEvento.VersionNueva, $"creó la versión {nueva.Numero} para cambiar el alcance.", ahora);
        return nueva;
    }

    public void EntregarHito(Guid hitoId, string evidenciaUrl, DateTimeOffset ahora)
    {
        HitoVigente(hitoId).Entregar(evidenciaUrl, ahora);
        ActualizadoEn = ahora;
        Registrar(Actor.Freelancer, TipoDeEvento.HitoEntregado, $"entregó «{TituloDe(hitoId)}» con evidencia: {evidenciaUrl}", ahora);
    }

    public void AprobarHito(Guid hitoId, DateTimeOffset ahora)
    {
        HitoVigente(hitoId).Aprobar(ahora);
        ActualizadoEn = ahora;
        Registrar(Actor.Cliente, TipoDeEvento.HitoAprobado, $"aprobó «{TituloDe(hitoId)}».", ahora);
    }

    public void PedirCambios(Guid hitoId, string comentario, DateTimeOffset ahora)
    {
        HitoVigente(hitoId).PedirCambios(comentario);
        ActualizadoEn = ahora;
        Registrar(Actor.Cliente, TipoDeEvento.CambiosSolicitados, $"pidió cambios en «{TituloDe(hitoId)}»: «{comentario.Trim()}»", ahora);
    }

    // Solo se paga un hito aprobado y nunca más de lo que falta. Al cubrirlo completo pasa a Pagado;
    // cuando todos los hitos vigentes están pagados, el proyecto se completa.
    public Pago RegistrarPago(Guid hitoId, decimal monto, DateOnly fecha, string? nota, DateTimeOffset ahora)
    {
        var hito = HitoVigente(hitoId);
        if (hito.Estado != EstadoHito.Aprobado)
            throw new ReglaDeNegocioException("Solo se puede registrar el pago de un hito aprobado.");
        if (monto <= 0 || decimal.Round(monto, 2) != monto)
            throw new ReglaDeNegocioException("El monto debe ser mayor que cero y tener como máximo dos decimales.");

        var montoDelHito = VersionVigente!.TerminosDe(hitoId)!.Monto;
        var pendiente = montoDelHito - PagadoDe(hitoId);
        if (monto > pendiente)
            throw new ReglaDeNegocioException(
                $"No puede ser mayor que lo pendiente: ${pendiente.ToString("N2", CultureInfo.InvariantCulture)}.");

        var pago = new Pago(OwnerId, Id, hitoId, monto, fecha, nota, ahora);
        _pagos.Add(pago);
        if (monto == pendiente) hito.MarcarPagado(ahora);

        Registrar(Actor.Freelancer, TipoDeEvento.PagoRegistrado,
            $"registró un pago de {EventoDeProyecto.Dinero(monto)} en «{TituloDe(hitoId)}»{(nota is null ? "" : $" ({nota})")}.", ahora);
        var vigentes = VersionVigente.Terminos.Select(t => t.HitoId).ToHashSet();
        if (_hitos.Where(h => vigentes.Contains(h.Id)).All(h => h.Estado == EstadoHito.Pagado))
        {
            Estado = EstadoProyecto.Completado;
            Registrar(Actor.Freelancer, TipoDeEvento.ProyectoCompletado, "completó el proyecto: todos los hitos están pagados.", ahora);
        }
        ActualizadoEn = ahora;
        return pago;
    }

    // Solo se trabaja sobre hitos del alcance aceptado y vigente.
    private HitoDeProyecto HitoVigente(Guid hitoId)
    {
        ExigirQueSigaAbierto();
        if (Estado != EstadoProyecto.Aceptado || VersionVigente is not { } vigente)
            throw new ReglaDeNegocioException("El cliente aún no acepta el alcance.");
        if (vigente.TerminosDe(hitoId) is null)
            throw new ReglaDeNegocioException("Ese hito no forma parte del alcance vigente.");
        return _hitos.First(h => h.Id == hitoId);
    }

    // Para dar un enlace nuevo sin cambiar el alcance: el proyecto ya debe haberse enviado.
    public void ExigirQuePuedaCompartirse()
    {
        ExigirQueSigaAbierto();
        if (Estado == EstadoProyecto.Borrador)
            throw new ReglaDeNegocioException("Primero envía el proyecto al cliente.");
    }

    // Los enlaces se crean y revocan fuera del agregado; aquí solo queda constancia en el historial.
    public void AnotarEnlaceGenerado(DateTimeOffset ahora)
    {
        ActualizadoEn = ahora;
        Registrar(Actor.Freelancer, TipoDeEvento.EnlaceGenerado, "generó un enlace nuevo para el cliente; el anterior dejó de funcionar.", ahora);
    }

    public void AnotarEnlaceRevocado(DateTimeOffset ahora)
    {
        ActualizadoEn = ahora;
        Registrar(Actor.Freelancer, TipoDeEvento.EnlaceRevocado, "revocó el enlace del cliente.", ahora);
    }

    // Un proyecto con pagos se puede cancelar: se conserva con todo su historial.
    public void Cancelar(DateTimeOffset ahora)
    {
        ExigirQueSigaAbierto();
        Estado = EstadoProyecto.Cancelado;
        ActualizadoEn = ahora;
        Registrar(Actor.Freelancer, TipoDeEvento.ProyectoCancelado, "canceló el proyecto.", ahora);
    }

    private void ExigirQueSigaAbierto()
    {
        if (Estado is EstadoProyecto.Cancelado or EstadoProyecto.Completado)
            throw new ReglaDeNegocioException(Estado == EstadoProyecto.Cancelado
                ? "El proyecto está cancelado."
                : "El proyecto ya está completado.");
    }
}
