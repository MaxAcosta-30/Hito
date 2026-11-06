using Hito.Domain.Comun;

namespace Hito.Domain.Proyectos;

// Una versión del alcance. Al aceptarse, sus términos quedan congelados; un cambio posterior crea otra versión.
public sealed class VersionDeProyecto : IPerteneceAUsuario
{
    private readonly List<TerminosDeHito> _terminos = [];

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public Guid ProyectoId { get; private set; }
    public int Numero { get; private set; }
    public EstadoVersion Estado { get; private set; } = EstadoVersion.Borrador;
    public DateTimeOffset CreadaEn { get; private set; }
    public DateTimeOffset? EnviadaEn { get; private set; }
    public DateTimeOffset? AceptadaEn { get; private set; }

    public IReadOnlyList<TerminosDeHito> Terminos => _terminos;

    // El total siempre se calcula aquí, nunca llega desde el cliente.
    public decimal Total => _terminos.Sum(t => t.Monto);

    private VersionDeProyecto() { }

    internal VersionDeProyecto(Guid ownerId, Guid proyectoId, int numero, DateTimeOffset ahora)
    {
        OwnerId = ownerId;
        ProyectoId = proyectoId;
        Numero = numero;
        CreadaEn = ahora;
    }

    public TerminosDeHito? TerminosDe(Guid hitoId) => _terminos.FirstOrDefault(t => t.HitoId == hitoId);

    internal void AgregarTerminos(Guid hitoId, int orden, DatosDeHito datos) =>
        _terminos.Add(new TerminosDeHito(OwnerId, Id, hitoId, orden, datos));

    internal void QuitarTerminos(TerminosDeHito terminos) => _terminos.Remove(terminos);
}
