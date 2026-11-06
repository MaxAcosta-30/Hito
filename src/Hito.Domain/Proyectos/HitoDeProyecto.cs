using Hito.Domain.Comun;

namespace Hito.Domain.Proyectos;

// Un hito conserva su identidad y su estado entre versiones; sus términos (título, monto, fecha) viven en cada versión.
// Estados: Pendiente → Entregado → Aprobado → Pagado. Desde Entregado puede pasar a CambiosSolicitados y volver a Entregado.
public sealed class HitoDeProyecto : IPerteneceAUsuario
{
    public const int MaximoUrl = 500;
    public const int MaximoComentario = 1000;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public Guid ProyectoId { get; private set; }
    public EstadoHito Estado { get; private set; } = EstadoHito.Pendiente;
    public string? EvidenciaUrl { get; private set; }
    public DateTimeOffset? EntregadoEn { get; private set; }
    public string? ComentarioCambios { get; private set; }
    public DateTimeOffset? AprobadoEn { get; private set; }
    public DateTimeOffset? PagadoEn { get; private set; }

    private HitoDeProyecto() { }

    internal HitoDeProyecto(Guid ownerId, Guid proyectoId)
    {
        OwnerId = ownerId;
        ProyectoId = proyectoId;
    }

    // Solo http y https: evita javascript:, data: y similares en un enlace que el cliente va a abrir.
    public static bool UrlValida(string? url) =>
        url is { Length: > 0 and <= MaximoUrl }
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);

    internal void Entregar(string evidenciaUrl, DateTimeOffset ahora)
    {
        if (Estado is not (EstadoHito.Pendiente or EstadoHito.CambiosSolicitados))
            throw new ReglaDeNegocioException($"Un hito {Nombre(Estado)} no se puede entregar.");
        if (!UrlValida(evidenciaUrl))
            throw new ReglaDeNegocioException("La evidencia debe ser una dirección http o https.");
        EvidenciaUrl = evidenciaUrl;
        EntregadoEn = ahora;
        Estado = EstadoHito.Entregado;
    }

    internal void Aprobar(DateTimeOffset ahora)
    {
        if (Estado != EstadoHito.Entregado)
            throw new ReglaDeNegocioException($"Un hito {Nombre(Estado)} no se puede aprobar.");
        ComentarioCambios = null;
        AprobadoEn = ahora;
        Estado = EstadoHito.Aprobado;
    }

    internal void PedirCambios(string comentario)
    {
        if (Estado != EstadoHito.Entregado)
            throw new ReglaDeNegocioException($"Solo se pueden pedir cambios a un hito entregado; este está {Nombre(Estado)}.");
        if (string.IsNullOrWhiteSpace(comentario) || comentario.Trim().Length > MaximoComentario)
            throw new ReglaDeNegocioException("Escribe qué hay que cambiar.");
        ComentarioCambios = comentario.Trim();
        Estado = EstadoHito.CambiosSolicitados;
    }

    internal void MarcarPagado(DateTimeOffset ahora)
    {
        if (Estado != EstadoHito.Aprobado)
            throw new ReglaDeNegocioException($"Solo se paga un hito aprobado; este está {Nombre(Estado)}.");
        PagadoEn = ahora;
        Estado = EstadoHito.Pagado;
    }

    private static string Nombre(EstadoHito estado) => estado switch
    {
        EstadoHito.Pendiente => "pendiente",
        EstadoHito.Entregado => "entregado",
        EstadoHito.CambiosSolicitados => "con cambios solicitados",
        EstadoHito.Aprobado => "aprobado",
        _ => "pagado",
    };
}
