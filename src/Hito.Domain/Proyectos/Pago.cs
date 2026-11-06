using Hito.Domain.Comun;

namespace Hito.Domain.Proyectos;

// Un pago manual (transferencia, efectivo…) registrado por el freelancer. No se edita ni se borra.
public sealed class Pago : IPerteneceAUsuario
{
    public const int MaximoNota = 200;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public Guid ProyectoId { get; private set; }
    public Guid HitoId { get; private set; }
    public decimal Monto { get; private set; }
    public DateOnly Fecha { get; private set; }
    public string? Nota { get; private set; }
    public DateTimeOffset RegistradoEn { get; private set; }

    private Pago() { }

    internal Pago(Guid ownerId, Guid proyectoId, Guid hitoId, decimal monto, DateOnly fecha, string? nota, DateTimeOffset ahora)
    {
        OwnerId = ownerId;
        ProyectoId = proyectoId;
        HitoId = hitoId;
        Monto = monto;
        Fecha = fecha;
        Nota = nota;
        RegistradoEn = ahora;
    }
}
