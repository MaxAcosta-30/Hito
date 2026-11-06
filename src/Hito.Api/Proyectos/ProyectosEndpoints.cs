using Hito.Api.Comun;
using Hito.Application.Comun;
using Hito.Domain.Proyectos;
using Hito.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hito.Api.Proyectos;

public static class ProyectosEndpoints
{
    public sealed record HitoSolicitud(Guid? Id, string? Titulo, decimal? Monto, DateOnly? FechaEntrega, string? Entregable);
    public sealed record CrearSolicitud(Guid? ClienteId, string? Nombre, List<HitoSolicitud>? Hitos);
    public sealed record AlcanceSolicitud(string? Nombre, List<HitoSolicitud>? Hitos);

    public sealed record ClienteResumen(Guid Id, string Nombre);
    public sealed record ProyectoResumen(
        Guid Id, string Nombre, ClienteResumen Cliente, EstadoProyecto Estado, decimal Total, int Version, DateTimeOffset ActualizadoEn);
    public sealed record HitoRespuesta(
        Guid Id, int Orden, string Titulo, decimal Monto, DateOnly FechaEntrega, string Entregable, EstadoHito Estado);
    public sealed record VersionRespuesta(
        int Numero, EstadoVersion Estado, DateTimeOffset CreadaEn, DateTimeOffset? EnviadaEn, DateTimeOffset? AceptadaEn,
        decimal Total, List<HitoRespuesta> Hitos);
    public sealed record ProyectoDetalle(
        Guid Id, string Nombre, ClienteResumen Cliente, EstadoProyecto Estado, decimal Total,
        int VersionActual, int UltimaVersion, List<VersionRespuesta> Versiones, EnlaceRespuesta? Enlace,
        DateTimeOffset CreadoEn, DateTimeOffset ActualizadoEn);
    // Datos del enlace activo. El token nunca se puede volver a leer: solo se muestra al crearlo.
    public sealed record EnlaceRespuesta(DateTimeOffset CreadoEn, DateTimeOffset ExpiraEn);
    public sealed record EnlaceCreado(string Token, DateTimeOffset ExpiraEn, ProyectoDetalle Proyecto);

    public static void MapProyectos(this IEndpointRouteBuilder app)
    {
        var proyectos = app.MapGroup("/proyectos").RequireAuthorization();
        proyectos.MapGet("/", Listar);
        proyectos.MapGet("/{id:guid}", Obtener);
        proyectos.MapPost("/", Crear);
        proyectos.MapPut("/{id:guid}/alcance", DefinirAlcance);
        proyectos.MapPost("/{id:guid}/cancelar", Cancelar);
        proyectos.MapPost("/{id:guid}/versiones", NuevaVersion);
        proyectos.MapPost("/{id:guid}/enviar", Enviar);
        proyectos.MapPost("/{id:guid}/enlace", NuevoEnlace);
        proyectos.MapPost("/{id:guid}/enlace/revocar", RevocarEnlace);
    }

    private static async Task<IResult> Listar(HitoDbContext db, CancellationToken ct)
    {
        var proyectos = await db.Proyectos.ConAgregado().OrderByDescending(p => p.ActualizadoEn).ToListAsync(ct);
        var clientes = await NombresDeClientesAsync(db, proyectos.Select(p => p.ClienteId), ct);
        return Results.Ok(proyectos.Select(p => new ProyectoResumen(
            p.Id, p.Nombre, new ClienteResumen(p.ClienteId, clientes[p.ClienteId]), p.Estado, p.Total,
            p.VersionActual.Numero, p.ActualizadoEn)));
    }

    private static async Task<IResult> Obtener(Guid id, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.ConAgregado().FirstOrDefaultAsync(p => p.Id == id, ct);
        return proyecto is null ? NoEncontrado() : Results.Ok(await DetalleAsync(db, proyecto, reloj.GetUtcNow(), ct));
    }

    private static async Task<IResult> Crear(
        CrearSolicitud solicitud, HitoDbContext db, IUsuarioActual usuario, TimeProvider reloj, CancellationToken ct)
    {
        var validacion = new Validacion();
        // El filtro por dueño hace que un cliente de otro usuario "no exista" aquí.
        var cliente = solicitud.ClienteId is { } clienteId
            ? await db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId, ct)
            : null;
        if (cliente is null) validacion.Agregar("clienteId", "Elige un cliente.");
        else if (cliente.Archivado) validacion.Agregar("clienteId", "Ese cliente está archivado. Restáuralo para crearle proyectos.");
        var (nombre, hitos) = Validar(solicitud.Nombre, solicitud.Hitos, validacion);
        if (validacion.HayErrores) return validacion.Problema();

        var proyecto = Proyecto.Crear(usuario.Id!.Value, cliente!.Id, nombre, hitos, reloj.GetUtcNow());
        db.Proyectos.Add(proyecto);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/proyectos/{proyecto.Id}", await DetalleAsync(db, proyecto, reloj.GetUtcNow(), ct));
    }

    private static async Task<IResult> DefinirAlcance(
        Guid id, AlcanceSolicitud solicitud, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.ConAgregado().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proyecto is null) return NoEncontrado();

        var validacion = new Validacion();
        var (nombre, hitos) = Validar(solicitud.Nombre, solicitud.Hitos, validacion);
        if (validacion.HayErrores) return validacion.Problema();

        proyecto.DefinirAlcance(nombre, hitos, reloj.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return Results.Ok(await DetalleAsync(db, proyecto, reloj.GetUtcNow(), ct));
    }

    private static async Task<IResult> Cancelar(Guid id, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.ConAgregado().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proyecto is null) return NoEncontrado();

        var ahora = reloj.GetUtcNow();
        proyecto.Cancelar(ahora);
        // Un proyecto cancelado ya no se comparte.
        await RevocarEnlacesAsync(db, proyecto.Id, ahora, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(await DetalleAsync(db, proyecto, ahora, ct));
    }

    private static async Task<IResult> NuevaVersion(Guid id, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.ConAgregado().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proyecto is null) return NoEncontrado();

        var ahora = reloj.GetUtcNow();
        proyecto.NuevaVersion(ahora);
        await db.SaveChangesAsync(ct);
        return Results.Ok(await DetalleAsync(db, proyecto, ahora, ct));
    }

    // Envía la versión en borrador y crea el enlace en la misma transacción (un solo SaveChanges).
    private static async Task<IResult> Enviar(Guid id, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.ConAgregado().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proyecto is null) return NoEncontrado();

        var ahora = reloj.GetUtcNow();
        proyecto.Enviar(ahora);
        var (token, enlace) = await CrearEnlaceAsync(db, proyecto, ahora, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new EnlaceCreado(token, enlace.ExpiraEn, await DetalleAsync(db, proyecto, ahora, ct)));
    }

    // Por si el cliente perdió el enlace o expiró: el anterior deja de servir.
    private static async Task<IResult> NuevoEnlace(Guid id, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.ConAgregado().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proyecto is null) return NoEncontrado();

        var ahora = reloj.GetUtcNow();
        proyecto.ExigirQuePuedaCompartirse();
        var (token, enlace) = await CrearEnlaceAsync(db, proyecto, ahora, ct);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new EnlaceCreado(token, enlace.ExpiraEn, await DetalleAsync(db, proyecto, ahora, ct)));
    }

    private static async Task<IResult> RevocarEnlace(Guid id, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        if (!await db.Proyectos.AnyAsync(p => p.Id == id, ct)) return NoEncontrado();
        await RevocarEnlacesAsync(db, id, reloj.GetUtcNow(), ct);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // Un solo enlace activo por proyecto: crear uno nuevo revoca los anteriores.
    private static async Task<(string Token, EnlaceDeCliente Enlace)> CrearEnlaceAsync(
        HitoDbContext db, Proyecto proyecto, DateTimeOffset ahora, CancellationToken ct)
    {
        await RevocarEnlacesAsync(db, proyecto.Id, ahora, ct);
        var token = TokenAleatorio.Generar();
        var enlace = new EnlaceDeCliente(proyecto.OwnerId, proyecto.Id, TokenAleatorio.Hash(token), ahora);
        db.Enlaces.Add(enlace);
        return (token, enlace);
    }

    private static async Task RevocarEnlacesAsync(HitoDbContext db, Guid proyectoId, DateTimeOffset ahora, CancellationToken ct)
    {
        var activos = await db.Enlaces.Where(e => e.ProyectoId == proyectoId && e.RevocadoEn == null).ToListAsync(ct);
        foreach (var enlace in activos) enlace.Revocar(ahora);
    }

    // Validación de entrada con los nombres de campo que usa la web: hitos[0].monto, etc.
    private static (string Nombre, List<DatosDeHito> Hitos) Validar(string? nombre, List<HitoSolicitud>? hitos, Validacion v)
    {
        var limpio = v.Requerido("nombre", nombre, Proyecto.MaximoNombre, $"Escribe el nombre del proyecto (máximo {Proyecto.MaximoNombre} caracteres).");
        var datos = new List<DatosDeHito>();
        if (hitos is null || hitos.Count is 0 or > DatosDeHito.MaximoHitos)
        {
            v.Agregar("hitos", $"Agrega entre 1 y {DatosDeHito.MaximoHitos} hitos.");
            return (limpio, datos);
        }

        foreach (var (h, i) in hitos.Select((h, i) => (h, i)))
        {
            var dato = new DatosDeHito(h.Id, h.Titulo?.Trim() ?? "", h.Monto ?? 0, h.FechaEntrega ?? default, h.Entregable?.Trim() ?? "");
            foreach (var (campo, mensaje) in dato.Errores()) v.Agregar($"hitos[{i}].{campo}", mensaje);
            datos.Add(dato);
        }
        return (limpio, datos);
    }

    internal static IQueryable<Proyecto> ConAgregado(this IQueryable<Proyecto> proyectos) =>
        proyectos
            .Include(p => p.Versiones).ThenInclude(v => v.Terminos)
            .Include(p => p.Hitos)
            .AsSplitQuery();

    internal static async Task<ProyectoDetalle> DetalleAsync(HitoDbContext db, Proyecto p, DateTimeOffset ahora, CancellationToken ct)
    {
        var clientes = await NombresDeClientesAsync(db, [p.ClienteId], ct);
        var enlace = await db.Enlaces
            .Where(e => e.ProyectoId == p.Id && e.RevocadoEn == null && e.ExpiraEn > ahora)
            .OrderByDescending(e => e.CreadoEn)
            .Select(e => new EnlaceRespuesta(e.CreadoEn, e.ExpiraEn))
            .FirstOrDefaultAsync(ct);
        var estados = p.Hitos.ToDictionary(h => h.Id, h => h.Estado);
        return new ProyectoDetalle(
            p.Id, p.Nombre, new ClienteResumen(p.ClienteId, clientes[p.ClienteId]), p.Estado, p.Total,
            p.VersionActual.Numero, p.UltimaVersion.Numero,
            p.Versiones.OrderByDescending(v => v.Numero).Select(v => new VersionRespuesta(
                v.Numero, v.Estado, v.CreadaEn, v.EnviadaEn, v.AceptadaEn, v.Total,
                v.Terminos.OrderBy(t => t.Orden)
                    .Select(t => new HitoRespuesta(t.HitoId, t.Orden, t.Titulo, t.Monto, t.FechaEntrega, t.Entregable, estados[t.HitoId]))
                    .ToList()))
                .ToList(),
            enlace, p.CreadoEn, p.ActualizadoEn);
    }

    // Incluye clientes archivados: un proyecto conserva el nombre de su cliente aunque se archive.
    private static Task<Dictionary<Guid, string>> NombresDeClientesAsync(HitoDbContext db, IEnumerable<Guid> ids, CancellationToken ct)
    {
        var lista = ids.Distinct().ToList();
        return db.Clientes.Where(c => lista.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Nombre, ct);
    }

    private static IResult NoEncontrado() =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "No encontramos ese proyecto.");
}
