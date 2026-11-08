using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Hito.IntegrationTests;

public sealed class AuthTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    private const string Contrasena = "contrasena-segura-123";
    private readonly HttpClient _cliente = fixture.Api.Cliente();

    private sealed record UsuarioDto(Guid Id, string Nombre, string Email);
    private sealed record SesionDto(string AccessToken, UsuarioDto Usuario);
    private sealed record Sesion(SesionDto Datos, string Cookie);

    [Fact]
    public async Task Registro_devuelve_un_token_que_identifica_al_usuario()
    {
        var email = NuevoEmail();

        var sesion = await RegistrarAsync(email);
        var yo = await YoAsync(sesion.Datos.AccessToken);

        Assert.Equal(HttpStatusCode.OK, yo.StatusCode);
        Assert.Equal(email, (await yo.Content.ReadFromJsonAsync<UsuarioDto>())!.Email);
    }

    [Fact]
    public async Task Cada_token_identifica_solo_a_su_usuario()
    {
        var a = await RegistrarAsync(NuevoEmail());
        var b = await RegistrarAsync(NuevoEmail());

        var yoConTokenDeB = await (await YoAsync(b.Datos.AccessToken)).Content.ReadFromJsonAsync<UsuarioDto>();

        Assert.Equal(b.Datos.Usuario.Id, yoConTokenDeB!.Id);
        Assert.NotEqual(a.Datos.Usuario.Id, yoConTokenDeB.Id);
    }

    [Fact]
    public async Task Entrar_con_la_contrasena_correcta_responde_200_y_con_otra_401()
    {
        var email = NuevoEmail();
        await RegistrarAsync(email);

        Assert.Equal(HttpStatusCode.OK, (await EntrarAsync(email, Contrasena)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await EntrarAsync(email, "otra-contrasena-123")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await EntrarAsync(NuevoEmail(), Contrasena)).StatusCode);
    }

    [Fact]
    public async Task Cinco_fallos_bloquean_la_cuenta_aunque_despues_la_contrasena_sea_correcta()
    {
        var email = NuevoEmail();
        await RegistrarAsync(email);

        for (var i = 0; i < 5; i++) await EntrarAsync(email, "otra-contrasena-123");

        Assert.Equal(HttpStatusCode.Unauthorized, (await EntrarAsync(email, Contrasena)).StatusCode);
    }

    [Fact]
    public async Task Yo_sin_token_o_con_token_alterado_responde_401()
    {
        var sesion = await RegistrarAsync(NuevoEmail());
        var alterado = sesion.Datos.AccessToken[..^4] + "AAAA";

        Assert.Equal(HttpStatusCode.Unauthorized, (await YoAsync(null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await YoAsync("no-es-un-jwt")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await YoAsync(alterado)).StatusCode);
    }

    [Fact]
    public async Task La_cookie_de_renovacion_es_httponly_secure_strict_y_limitada_a_auth()
    {
        var respuesta = await _cliente.PostAsJsonAsync("/auth/registro",
            new { nombre = "Max", email = NuevoEmail(), contrasena = Contrasena });

        var cookie = respuesta.Headers.GetValues("Set-Cookie").Single().ToLowerInvariant();

        Assert.Contains("httponly", cookie);
        Assert.Contains("secure", cookie);
        Assert.Contains("samesite=strict", cookie);
        Assert.Contains("path=/api/auth", cookie);
    }

    [Fact]
    public async Task Renovar_rota_el_token_y_reutilizar_uno_viejo_cierra_todas_las_sesiones()
    {
        var sesion = await RegistrarAsync(NuevoEmail());

        var primera = await RenovarAsync(sesion.Cookie);
        Assert.Equal(HttpStatusCode.OK, primera.StatusCode);
        var nuevaCookie = CookieDe(primera);
        Assert.NotEqual(sesion.Cookie, nuevaCookie);

        // El token original ya se usó: reutilizarlo es señal de robo.
        Assert.Equal(HttpStatusCode.Unauthorized, (await RenovarAsync(sesion.Cookie)).StatusCode);
        // Y como respuesta se revocó también el token nuevo.
        Assert.Equal(HttpStatusCode.Unauthorized, (await RenovarAsync(nuevaCookie)).StatusCode);
    }

    [Fact]
    public async Task Salir_revoca_la_sesion()
    {
        var sesion = await RegistrarAsync(NuevoEmail());

        var salir = new HttpRequestMessage(HttpMethod.Post, "/auth/salir");
        salir.Headers.Add("Cookie", $"hito_renovacion={sesion.Cookie}");
        Assert.Equal(HttpStatusCode.NoContent, (await _cliente.SendAsync(salir)).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await RenovarAsync(sesion.Cookie)).StatusCode);
    }

    [Fact]
    public async Task Registro_con_datos_invalidos_o_correo_repetido_responde_400()
    {
        var email = NuevoEmail();
        await RegistrarAsync(email);

        var corta = await _cliente.PostAsJsonAsync("/auth/registro", new { nombre = "Max", email = NuevoEmail(), contrasena = "corta" });
        var malCorreo = await _cliente.PostAsJsonAsync("/auth/registro", new { nombre = "Max", email = "no-es-correo", contrasena = Contrasena });
        var repetido = await _cliente.PostAsJsonAsync("/auth/registro", new { nombre = "Max", email, contrasena = Contrasena });

        Assert.Equal(HttpStatusCode.BadRequest, corta.StatusCode);
        Assert.Contains("contrasena", await corta.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.BadRequest, malCorreo.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, repetido.StatusCode);
        Assert.Contains("Ya existe una cuenta", await repetido.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Entrar_tiene_limite_por_ip_y_cada_ip_cuenta_aparte()
    {
        await using var app = fixture.Api.WithWebHostBuilder(b => b.UseSetting("Auth:EntradasPorMinuto", "2"));
        var api = app.CreateClient();
        Task<HttpResponseMessage> Entrar(string ip)
        {
            var solicitud = new HttpRequestMessage(HttpMethod.Post, "/auth/entrar")
            {
                Content = JsonContent.Create(new { email = NuevoEmail(), contrasena = "no-es-la-buena-123" }),
            };
            solicitud.Headers.Add("X-Forwarded-For", ip);
            return api.SendAsync(solicitud);
        }

        var codigos = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++) codigos.Add((await Entrar("203.0.113.7")).StatusCode);
        var otraIp = (await Entrar("198.51.100.20")).StatusCode;

        Assert.Equal([HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.TooManyRequests], codigos);
        Assert.Equal(HttpStatusCode.Unauthorized, otraIp);
    }

    [Fact]
    public async Task Renovar_tiene_limite_por_ip()
    {
        await using var app = fixture.Api.WithWebHostBuilder(b => b.UseSetting("Auth:RenovacionesPorMinuto", "1"));
        var api = app.CreateClient();

        var primera = await api.PostAsync("/auth/renovar", null);
        var segunda = await api.PostAsync("/auth/renovar", null);

        Assert.Equal(HttpStatusCode.Unauthorized, primera.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, segunda.StatusCode);
    }

    private static string NuevoEmail() => $"{Guid.NewGuid():N}@prueba.mx";

    private async Task<Sesion> RegistrarAsync(string email)
    {
        var respuesta = await _cliente.PostAsJsonAsync("/auth/registro", new { nombre = "Max", email, contrasena = Contrasena });
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        return new Sesion((await respuesta.Content.ReadFromJsonAsync<SesionDto>())!, CookieDe(respuesta));
    }

    private Task<HttpResponseMessage> EntrarAsync(string email, string contrasena) =>
        _cliente.PostAsJsonAsync("/auth/entrar", new { email, contrasena });

    private Task<HttpResponseMessage> YoAsync(string? token)
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Get, "/auth/yo");
        if (token is not null) solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _cliente.SendAsync(solicitud);
    }

    private Task<HttpResponseMessage> RenovarAsync(string cookie)
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Post, "/auth/renovar");
        solicitud.Headers.Add("Cookie", $"hito_renovacion={cookie}");
        return _cliente.SendAsync(solicitud);
    }

    private static string CookieDe(HttpResponseMessage respuesta) =>
        respuesta.Headers.GetValues("Set-Cookie").Single().Split(';')[0].Split('=', 2)[1];
}
