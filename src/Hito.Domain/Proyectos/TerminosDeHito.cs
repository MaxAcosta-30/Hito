using Hito.Domain.Comun;

namespace Hito.Domain.Proyectos;

// Los términos de un hito dentro de una versión. Solo cambian mientras la versión es borrador.
public sealed class TerminosDeHito : IPerteneceAUsuario
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public Guid VersionId { get; private set; }
    public Guid HitoId { get; private set; }
    public int Orden { get; private set; }
    public string Titulo { get; private set; } = null!;
    public decimal Monto { get; private set; }
    public DateOnly FechaEntrega { get; private set; }
    public string Entregable { get; private set; } = null!;

    private TerminosDeHito() { }

    internal TerminosDeHito(Guid ownerId, Guid versionId, Guid hitoId, int orden, DatosDeHito datos)
    {
        OwnerId = ownerId;
        VersionId = versionId;
        HitoId = hitoId;
        Actualizar(orden, datos);
    }

    internal void Actualizar(int orden, DatosDeHito datos)
    {
        Orden = orden;
        Titulo = datos.Titulo.Trim();
        Monto = datos.Monto;
        FechaEntrega = datos.FechaEntrega;
        Entregable = datos.Entregable.Trim();
    }

    internal bool MismosTerminos(TerminosDeHito otro) =>
        Titulo == otro.Titulo && Monto == otro.Monto && FechaEntrega == otro.FechaEntrega && Entregable == otro.Entregable;

    internal bool MismosTerminos(DatosDeHito datos) =>
        Titulo == datos.Titulo.Trim() && Monto == datos.Monto && FechaEntrega == datos.FechaEntrega && Entregable == datos.Entregable.Trim();
}
