using System.Text.RegularExpressions;
using Hito.Api.Comun;
using Hito.Application.Comun;
using Hito.Domain.Clientes;
using Hito.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hito.Api.Clientes;

public static partial class ClientesEndpoints
{
    public sealed record ClienteSolicitud(string? Nombre, string? Empresa, string? Email, string? Telefono);
    public sealed record ClienteRespuesta(
        Guid Id, string Nombre, string? Empresa, string? Email, string? Telefono, DateTimeOffset CreadoEn, DateTimeOffset? ArchivadoEn);

    public static void MapClientes(this IEndpointRouteBuilder app)
    {
        var clientes = app.MapGroup("/clientes").RequireAuthorization();
        clientes.MapGet("/", Listar);
        clientes.MapGet("/{id:guid}", Obtener);
        clientes.MapPost("/", Crear);
        clientes.MapPut("/{id:guid}", Actualizar);
        clientes.MapPost("/{id:guid}/archivar", Archivar);
        clientes.MapPost("/{id:guid}/restaurar", Restaurar);
    }

    // El filtro por dueño de HitoDbContext ya limita cada consulta a los clientes del usuario actual.
    private static async Task<IResult> Listar(HitoDbContext db, CancellationToken ct, bool archivados = false)
    {
        var lista = await db.Clientes
            .Where(c => archivados ? c.ArchivadoEn != null : c.ArchivadoEn == null)
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);
        return Results.Ok(lista.Select(ARespuesta));
    }

    // Un cliente de otro usuario responde 404, no 403: así no se confirma que el id existe.
    private static async Task<IResult> Obtener(Guid id, HitoDbContext db, CancellationToken ct) =>
        await db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct) is { } cliente
            ? Results.Ok(ARespuesta(cliente))
            : NoEncontrado();

    private static async Task<IResult> Crear(
        ClienteSolicitud solicitud, HitoDbContext db, IUsuarioActual usuario, TimeProvider reloj, CancellationToken ct)
    {
        var validacion = new Validacion();
        var datos = Validar(solicitud, validacion);
        if (validacion.HayErrores) return validacion.Problema();

        var cliente = new Cliente(usuario.Id!.Value, datos.Nombre, datos.Empresa, datos.Email, datos.Telefono, reloj.GetUtcNow());
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/clientes/{cliente.Id}", ARespuesta(cliente));
    }

    private static async Task<IResult> Actualizar(Guid id, ClienteSolicitud solicitud, HitoDbContext db, CancellationToken ct)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cliente is null) return NoEncontrado();

        var validacion = new Validacion();
        var datos = Validar(solicitud, validacion);
        if (validacion.HayErrores) return validacion.Problema();

        cliente.Actualizar(datos.Nombre, datos.Empresa, datos.Email, datos.Telefono);
        await db.SaveChangesAsync(ct);
        return Results.Ok(ARespuesta(cliente));
    }

    private static async Task<IResult> Archivar(Guid id, HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cliente is null) return NoEncontrado();
        cliente.Archivar(reloj.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return Results.Ok(ARespuesta(cliente));
    }

    private static async Task<IResult> Restaurar(Guid id, HitoDbContext db, CancellationToken ct)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (cliente is null) return NoEncontrado();
        cliente.Restaurar();
        await db.SaveChangesAsync(ct);
        return Results.Ok(ARespuesta(cliente));
    }

    private sealed record DatosValidos(string Nombre, string? Empresa, string? Email, string? Telefono);

    private static DatosValidos Validar(ClienteSolicitud s, Validacion v)
    {
        var nombre = v.Requerido("nombre", s.Nombre, 120, "Escribe el nombre del cliente (máximo 120 caracteres).");
        var empresa = v.Opcional("empresa", s.Empresa, 120, "Máximo 120 caracteres.");
        var email = v.EmailOpcional("email", s.Email);
        var telefono = v.Opcional("telefono", s.Telefono, 30, "Máximo 30 caracteres.");
        if (telefono is not null && !Telefono().IsMatch(telefono))
            v.Agregar("telefono", "Usa solo números, espacios y los signos + - ( ).");
        return new DatosValidos(nombre, empresa, email, telefono);
    }

    [GeneratedRegex(@"^\+?[0-9 ()\-]{7,30}$")]
    private static partial Regex Telefono();

    private static IResult NoEncontrado() =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "No encontramos ese cliente.");

    private static ClienteRespuesta ARespuesta(Cliente c) =>
        new(c.Id, c.Nombre, c.Empresa, c.Email, c.Telefono, c.CreadoEn, c.ArchivadoEn);
}
