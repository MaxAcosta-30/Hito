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
        int VersionActual, int UltimaVersion, List<VersionRespuesta> Versiones, DateTimeOffset CreadoEn, DateTimeOffset ActualizadoEn);

    public static void MapProyectos(this IEndpointRouteBuilder app)
    {
        var proyectos = app.MapGroup("/proyectos").RequireAuthorization();
        proyectos.MapGet("/", Listar);
        proyectos.MapGet("/{id:guid}", Obtener);
        proyectos.MapPost("/", Crear);
        proyectos.MapPut("/{id:guid}/alcance", DefinirAlcance);
        proyectos.MapPost("/{id:guid}/cancelar", Cancelar);
    }

    private static async Task<IResult> Listar(HitoDbContext db, CancellationToken ct)
    {
        var proyectos = await db.Proyectos.ConAgregado().OrderByDescending(p => p.ActualizadoEn).ToListAsync(ct);
        var clientes = await NombresDeClientesAsync(db, proyectos.Select(p => p.ClienteId), ct);
        return Results.Ok(proyectos.Select(p => new ProyectoResumen(
            p.Id, p.Nombre, new ClienteResumen(p.ClienteId, clientes[p.ClienteId]), p.Estado, p.Total,
            p.VersionActual.Numero, p.ActualizadoEn)));
    }

    private static async Task<IResult> Obtener(Guid id, HitoDbContext db, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.ConAgregado().FirstOrDefaultAsync(p => p.Id == id, ct);
        return proyecto is null ? NoEncontrado() : Results.Ok(await DetalleAsync(db, proyecto, ct));
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
        return Results.Created($"/proyectos/{proyecto.Id}", await DetalleAsync(db, proyecto, ct));
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
        return Results.Ok(await DetalleAsync(db, proyecto, ct));
    }

    private static async Task<IResult> Cancelar(Guid id, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var proyecto = await db.Proyectos.ConAgregado().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proyecto is null) return NoEncontrado();

        proyecto.Cancelar(reloj.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return Results.Ok(await DetalleAsync(db, proyecto, ct));
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

    internal static async Task<ProyectoDetalle> DetalleAsync(HitoDbContext db, Proyecto p, CancellationToken ct)
    {
        var clientes = await NombresDeClientesAsync(db, [p.ClienteId], ct);
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
            p.CreadoEn, p.ActualizadoEn);
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
