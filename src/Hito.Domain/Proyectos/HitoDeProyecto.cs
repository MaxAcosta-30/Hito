using Hito.Domain.Comun;

namespace Hito.Domain.Proyectos;

// Un hito conserva su identidad y su estado entre versiones; sus términos (título, monto, fecha) viven en cada versión.
public sealed class HitoDeProyecto : IPerteneceAUsuario
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public Guid ProyectoId { get; private set; }
    public EstadoHito Estado { get; private set; } = EstadoHito.Pendiente;

    private HitoDeProyecto() { }

    internal HitoDeProyecto(Guid ownerId, Guid proyectoId)
    {
        OwnerId = ownerId;
        ProyectoId = proyectoId;
    }
}
