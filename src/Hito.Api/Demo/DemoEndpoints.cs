using Hito.Api.Auth;
using Hito.Api.Comun;
using Hito.Infrastructure.Identidad;
using Hito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hito.Api.Demo;

// "Probar la demo": con un clic crea una cuenta temporal con datos de ejemplo y abre su sesión.
// Cada demo es un usuario distinto, así el filtro por dueño la aísla de las demás.
public static class DemoEndpoints
{
    public const string Politica = "demo";
    public static readonly TimeSpan Duracion = TimeSpan.FromHours(24);

    public static void MapDemo(this IEndpointRouteBuilder app) =>
        app.MapPost("/demo", Crear).RequireRateLimiting(Politica);

    private static async Task<IResult> Crear(
        HitoDbContext db, UserManager<Usuario> usuarios, Sesiones sesiones, EmisorDeTokens emisor,
        HttpContext http, IConfiguration config, TimeProvider reloj, CancellationToken ct)
    {
        var ahora = reloj.GetUtcNow();

        // Limpieza: borrar el usuario borra en cascada sus clientes, proyectos, pagos e historial.
        // Se hace aquí y no con un job programado porque Render Free se duerme y el job no correría con certeza.
        await db.Users.Where(u => u.DemoExpiraEn != null && u.DemoExpiraEn < ahora).ExecuteDeleteAsync(ct);

        var email = $"demo-{Guid.NewGuid():N}@demo.hito.mx";
        // Sin contraseña: nadie puede entrar con "Entrar"; solo con la sesión que se abre aquí.
        var usuario = new Usuario { Nombre = "Lucía Herrera", Email = email, UserName = email, DemoExpiraEn = ahora + Duracion };
        var creado = await usuarios.CreateAsync(usuario);
        if (!creado.Succeeded)
            throw new InvalidOperationException("No se pudo crear la cuenta de demo: " + string.Join(", ", creado.Errors.Select(e => e.Code)));

        DatosDeDemo.Sembrar(db, usuario.Id, Calendario.Hoy(reloj), ahora);
        await db.SaveChangesAsync(ct);

        return Results.Created("/auth/yo", await AuthEndpoints.IniciarSesionAsync(usuario, sesiones, emisor, http, config, ct));
    }
}
