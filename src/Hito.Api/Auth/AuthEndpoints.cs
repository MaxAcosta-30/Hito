using System.Net.Mail;
using Hito.Application.Comun;
using Hito.Infrastructure.Identidad;
using Microsoft.AspNetCore.Identity;

namespace Hito.Api.Auth;

public static class AuthEndpoints
{
    public const string NombreCookie = "hito_renovacion";
    public const string PoliticaRegistro = "registro";

    public sealed record RegistroSolicitud(string? Nombre, string? Email, string? Contrasena);
    public sealed record EntrarSolicitud(string? Email, string? Contrasena);
    public sealed record UsuarioRespuesta(Guid Id, string Nombre, string Email);
    public sealed record SesionRespuesta(string AccessToken, UsuarioRespuesta Usuario);

    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/auth");
        auth.MapPost("/registro", Registrar).RequireRateLimiting(PoliticaRegistro);
        auth.MapPost("/entrar", Entrar);
        auth.MapPost("/renovar", Renovar);
        auth.MapPost("/salir", Salir);
        auth.MapGet("/yo", Yo).RequireAuthorization();
    }

    private static async Task<IResult> Registrar(
        RegistroSolicitud solicitud, UserManager<Usuario> usuarios, Sesiones sesiones, EmisorDeTokens emisor,
        HttpContext http, IConfiguration config, CancellationToken ct)
    {
        var nombre = solicitud.Nombre?.Trim() ?? "";
        var email = solicitud.Email?.Trim().ToLowerInvariant() ?? "";
        var errores = new Dictionary<string, string[]>();
        if (nombre.Length is 0 or > 100) errores["nombre"] = ["Escribe tu nombre (máximo 100 caracteres)."];
        if (email.Length > 256 || !MailAddress.TryCreate(email, out _)) errores["email"] = ["El correo no es válido."];
        if (string.IsNullOrEmpty(solicitud.Contrasena)) errores["contrasena"] = ["Escribe una contraseña."];
        if (errores.Count > 0) return Results.ValidationProblem(errores);

        var usuario = new Usuario { Nombre = nombre, Email = email, UserName = email };
        var resultado = await usuarios.CreateAsync(usuario, solicitud.Contrasena!);
        if (!resultado.Succeeded)
        {
            return Results.ValidationProblem(resultado.Errors
                .GroupBy(e => e.Code.StartsWith("Password", StringComparison.Ordinal) ? "contrasena" : "email")
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
        }

        return Results.Created("/auth/yo", await IniciarSesionAsync(usuario, sesiones, emisor, http, config, ct));
    }

    private static async Task<IResult> Entrar(
        EntrarSolicitud solicitud, UserManager<Usuario> usuarios, Sesiones sesiones, EmisorDeTokens emisor,
        HttpContext http, IConfiguration config, CancellationToken ct)
    {
        // Mismo mensaje para correo inexistente, contraseña incorrecta o cuenta bloqueada: no revela qué cuentas existen.
        var incorrectas = Results.Problem(statusCode: StatusCodes.Status401Unauthorized,
            title: "Correo o contraseña incorrectos. Tras varios intentos fallidos, espera 5 minutos.");

        var usuario = await usuarios.FindByEmailAsync(solicitud.Email?.Trim() ?? "");
        if (usuario is null || string.IsNullOrEmpty(solicitud.Contrasena)) return incorrectas;
        if (await usuarios.IsLockedOutAsync(usuario)) return incorrectas;

        if (!await usuarios.CheckPasswordAsync(usuario, solicitud.Contrasena))
        {
            await usuarios.AccessFailedAsync(usuario);
            return incorrectas;
        }

        await usuarios.ResetAccessFailedCountAsync(usuario);
        return Results.Ok(await IniciarSesionAsync(usuario, sesiones, emisor, http, config, ct));
    }

    private static async Task<IResult> Renovar(
        UserManager<Usuario> usuarios, Sesiones sesiones, EmisorDeTokens emisor,
        HttpContext http, IConfiguration config, CancellationToken ct)
    {
        var token = http.Request.Cookies[NombreCookie];
        var rotado = string.IsNullOrEmpty(token) ? null : await sesiones.RotarAsync(token, ct);
        var usuario = rotado is null ? null : await usuarios.FindByIdAsync(rotado.Value.UsuarioId.ToString());
        if (rotado is null || usuario is null)
        {
            BorrarCookie(http, config);
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "La sesión expiró. Vuelve a entrar.");
        }

        GuardarCookie(http, config, rotado.Value.Token, rotado.Value.ExpiraEn);
        return Results.Ok(new SesionRespuesta(emisor.Emitir(usuario), ARespuesta(usuario)));
    }

    private static async Task<IResult> Salir(Sesiones sesiones, HttpContext http, IConfiguration config, CancellationToken ct)
    {
        var token = http.Request.Cookies[NombreCookie];
        if (!string.IsNullOrEmpty(token)) await sesiones.RevocarAsync(token, ct);
        BorrarCookie(http, config);
        return Results.NoContent();
    }

    private static async Task<IResult> Yo(IUsuarioActual actual, UserManager<Usuario> usuarios)
    {
        var usuario = actual.Id is { } id ? await usuarios.FindByIdAsync(id.ToString()) : null;
        return usuario is null ? Results.Unauthorized() : Results.Ok(ARespuesta(usuario));
    }

    private static async Task<SesionRespuesta> IniciarSesionAsync(
        Usuario usuario, Sesiones sesiones, EmisorDeTokens emisor, HttpContext http, IConfiguration config, CancellationToken ct)
    {
        var (token, expira) = await sesiones.CrearAsync(usuario.Id, ct);
        GuardarCookie(http, config, token, expira);
        return new SesionRespuesta(emisor.Emitir(usuario), ARespuesta(usuario));
    }

    private static UsuarioRespuesta ARespuesta(Usuario u) => new(u.Id, u.Nombre, u.Email!);

    // httpOnly: JavaScript no puede leerla. Strict: el navegador no la manda desde otros sitios (CSRF).
    // La ruta es la que ve el navegador (/api/auth en Vercel), así la cookie no viaja en el resto de peticiones.
    private static CookieOptions OpcionesCookie(IConfiguration config) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = config["Auth:RutaCookie"] ?? "/api/auth",
        IsEssential = true,
    };

    private static void GuardarCookie(HttpContext http, IConfiguration config, string token, DateTimeOffset expira)
    {
        var opciones = OpcionesCookie(config);
        opciones.Expires = expira;
        http.Response.Cookies.Append(NombreCookie, token, opciones);
    }

    private static void BorrarCookie(HttpContext http, IConfiguration config) =>
        http.Response.Cookies.Delete(NombreCookie, OpcionesCookie(config));
}
