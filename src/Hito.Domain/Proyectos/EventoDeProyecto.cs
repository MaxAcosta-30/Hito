using System.Globalization;
using Hito.Domain.Comun;

namespace Hito.Domain.Proyectos;

public enum Actor { Freelancer, Cliente }

public enum TipoDeEvento
{
    Creado, AlcanceEditado, VersionEnviada, EnlaceGenerado, EnlaceRevocado, VersionAceptada, VersionNueva,
    HitoEntregado, CambiosSolicitados, HitoAprobado, PagoRegistrado, ProyectoCompletado, ProyectoCancelado,
}

// Una línea del historial: quién, qué y cuándo. Solo se agrega; nunca se edita ni se borra.
// La descripción se guarda como texto para que el historial cuente lo que pasó aunque después cambie un título.
public sealed class EventoDeProyecto : IPerteneceAUsuario
{
    public const int MaximoDescripcion = 1300;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public Guid ProyectoId { get; private set; }
    public DateTimeOffset Fecha { get; private set; }
    public Actor Actor { get; private set; }
    public TipoDeEvento Tipo { get; private set; }
    public string Descripcion { get; private set; } = null!;

    private EventoDeProyecto() { }

    internal EventoDeProyecto(Guid ownerId, Guid proyectoId, Actor actor, TipoDeEvento tipo, string descripcion, DateTimeOffset fecha)
    {
        OwnerId = ownerId;
        ProyectoId = proyectoId;
        Actor = actor;
        Tipo = tipo;
        Descripcion = descripcion.Length > MaximoDescripcion ? descripcion[..MaximoDescripcion] : descripcion;
        Fecha = fecha;
    }

    internal static string Dinero(decimal monto) => "$" + monto.ToString("N2", CultureInfo.InvariantCulture);
}
