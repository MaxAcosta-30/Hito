using Hito.Api.Comun;
using Hito.Api.Proyectos;
using Hito.Domain.Proyectos;
using Hito.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hito.Api.Publico;

// Lo que ve el cliente con su enlace, sin cuenta. Aquí no hay usuario autenticado, así que se ignora a propósito
// el filtro por dueño: el token es la autorización y solo da acceso al proyecto de ese enlace.
public static class PublicoEndpoints
{
    public const string Politica = "publico";

    public sealed record HitoPublico(Guid Id, string Titulo, decimal Monto, DateOnly FechaEntrega, string Entregable, EstadoHito Estado);
    public sealed record VersionPublica(int Numero, DateTimeOffset? EnviadaEn, DateTimeOffset? AceptadaEn, decimal Total, List<HitoPublico> Hitos);
    public sealed record VistaCliente(
        string Freelancer, string Proyecto, string Cliente, EstadoProyecto Estado,
        VersionPublica? Vigente, VersionPublica? PorAceptar, DateTimeOffset EnlaceExpiraEn);

    public sealed record AceptarSolicitud(int? Version);

    public static void MapPublico(this IEndpointRouteBuilder app)
    {
        // 43 caracteres: 32 bytes en base64url. Otra longitud ni siquiera llega a la base.
        var publico = app.MapGroup("/publico/{token:length(43)}").RequireRateLimiting(Politica);
        publico.MapGet("/", Ver);
        publico.MapPost("/aceptar", Aceptar);
    }

    private static async Task<IResult> Ver(string token, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var ahora = reloj.GetUtcNow();
        var proyecto = await ProyectoDelEnlaceAsync(db, token, ahora, ct);
        return proyecto is null ? EnlaceNoValido() : Results.Ok(await VistaAsync(db, proyecto.Value.Proyecto, proyecto.Value.ExpiraEn, ct));
    }

    private static async Task<IResult> Aceptar(
        string token, AceptarSolicitud solicitud, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var ahora = reloj.GetUtcNow();
        var encontrado = await ProyectoDelEnlaceAsync(db, token, ahora, ct);
        if (encontrado is null) return EnlaceNoValido();
        if (solicitud.Version is not { } version)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["version"] = ["Indica qué versión aceptas."] });

        var (proyecto, expiraEn) = encontrado.Value;
        proyecto.Aceptar(version, ahora);
        await db.SaveChangesAsync(ct);
        return Results.Ok(await VistaAsync(db, proyecto, expiraEn, ct));
    }

    internal static async Task<(Proyecto Proyecto, DateTimeOffset ExpiraEn)?> ProyectoDelEnlaceAsync(
        HitoDbContext db, string token, DateTimeOffset ahora, CancellationToken ct)
    {
        var hash = TokenAleatorio.Hash(token);
        var enlace = await db.Enlaces.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.HashDelToken == hash, ct);
        if (enlace is null || !enlace.EsValido(ahora)) return null;

        var proyecto = await db.Proyectos.IgnoreQueryFilters().ConAgregado()
            .FirstOrDefaultAsync(p => p.Id == enlace.ProyectoId, ct);
        return proyecto is null ? null : (proyecto, enlace.ExpiraEn);
    }

    internal static async Task<VistaCliente> VistaAsync(HitoDbContext db, Proyecto p, DateTimeOffset expiraEn, CancellationToken ct)
    {
        var freelancer = await db.Users.Where(u => u.Id == p.OwnerId).Select(u => u.Nombre).SingleAsync(ct);
        var cliente = await db.Clientes.IgnoreQueryFilters().Where(c => c.Id == p.ClienteId).Select(c => c.Nombre).SingleAsync(ct);
        var estados = p.Hitos.ToDictionary(h => h.Id, h => h.Estado);
        VersionPublica? Publica(VersionDeProyecto? v) => v is null ? null : new VersionPublica(
            v.Numero, v.EnviadaEn, v.AceptadaEn, v.Total,
            v.Terminos.OrderBy(t => t.Orden)
                .Select(t => new HitoPublico(t.HitoId, t.Titulo, t.Monto, t.FechaEntrega, t.Entregable, estados[t.HitoId]))
                .ToList());

        // El cliente nunca ve borradores: solo la versión aceptada y la que espera su respuesta.
        var porAceptar = p.UltimaVersion.Estado == EstadoVersion.Enviada ? p.UltimaVersion : null;
        return new VistaCliente(freelancer, p.Nombre, cliente, p.Estado, Publica(p.VersionVigente), Publica(porAceptar), expiraEn);
    }

    // Mismo 404 para un token inexistente, expirado o revocado: no se revela cuál de los tres es.
    internal static IResult EnlaceNoValido() =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Este enlace ya no es válido.");
}
