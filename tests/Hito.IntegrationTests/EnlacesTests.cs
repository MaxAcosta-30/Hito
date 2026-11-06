using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hito.Infrastructure.Persistence;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static Hito.IntegrationTests.Proyectos;

namespace Hito.IntegrationTests;

public sealed class EnlacesTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    private sealed record EnlaceCreadoDto(string Token, DateTimeOffset ExpiraEn, ProyectoDto Proyecto);

    [Fact]
    public async Task Enviar_crea_un_enlace_privado_y_en_la_base_solo_queda_su_hash()
    {
        var max = await fixture.Api.NuevoAsync();
        var proyecto = await CrearAsync(max);

        var enviado = await EnviarAsync(max, proyecto.Id);

        Assert.Equal("Enviado", enviado.Proyecto.Estado);
        Assert.Equal(43, enviado.Token.Length);
        var vista = await fixture.Api.Cliente().GetFromJsonAsync<JsonElement>($"/publico/{enviado.Token}");
        Assert.Equal("Tienda en línea", vista.GetProperty("proyecto").GetString());
        Assert.Equal("Max", vista.GetProperty("freelancer").GetString());
        Assert.Equal(20000m, vista.GetProperty("porAceptar").GetProperty("total").GetDecimal());

        using var scope = fixture.Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HitoDbContext>();
        var hashes = await db.Enlaces.IgnoreQueryFilters().Where(e => e.ProyectoId == proyecto.Id).Select(e => e.HashDelToken).ToListAsync();
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(enviado.Token)));
        Assert.Equal([hash], hashes);
        Assert.DoesNotContain(enviado.Token, hashes);
    }

    [Fact]
    public async Task Un_enlace_revocado_reemplazado_o_inventado_responde_404()
    {
        var max = await fixture.Api.NuevoAsync();
        var proyecto = await CrearAsync(max);
        var primero = await EnviarAsync(max, proyecto.Id);
        var anonimo = fixture.Api.Cliente();

        var nuevo = await max.PostAsync($"/proyectos/{proyecto.Id}/enlace", null);
        var segundo = (await nuevo.Content.ReadFromJsonAsync<EnlaceCreadoDto>(Json))!;
        Assert.Equal(HttpStatusCode.NotFound, (await anonimo.GetAsync($"/publico/{primero.Token}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await anonimo.GetAsync($"/publico/{segundo.Token}")).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await max.PostAsync($"/proyectos/{proyecto.Id}/enlace/revocar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonimo.GetAsync($"/publico/{segundo.Token}")).StatusCode);

        var inventado = new string('A', 43);
        Assert.Equal(HttpStatusCode.NotFound, (await anonimo.GetAsync($"/publico/{inventado}")).StatusCode);
        Assert.Null((await max.GetFromJsonAsync<JsonElement>($"/proyectos/{proyecto.Id}")).GetProperty("enlace").GetString());
    }

    [Fact]
    public async Task Un_enlace_expira_a_los_30_dias()
    {
        var reloj = new RelojAjustable(DateTimeOffset.UtcNow);
        await using var api = fixture.Api.WithWebHostBuilder(b =>
            b.ConfigureTestServices(s => s.AddSingleton<TimeProvider>(reloj)));
        var max = await NuevoUsuarioAsync(api);
        var proyecto = await CrearAsync(max);
        var enlace = await EnviarAsync(max, proyecto.Id);
        var anonimo = api.CreateClient();

        reloj.Avanzar(TimeSpan.FromDays(29));
        Assert.Equal(HttpStatusCode.OK, (await anonimo.GetAsync($"/publico/{enlace.Token}")).StatusCode);
        reloj.Avanzar(TimeSpan.FromDays(1));
        Assert.Equal(HttpStatusCode.NotFound, (await anonimo.GetAsync($"/publico/{enlace.Token}")).StatusCode);
    }

    [Fact]
    public async Task Reglas_de_envio_responden_409()
    {
        var max = await fixture.Api.NuevoAsync();
        var proyecto = await CrearAsync(max);

        // Sin enviar no se puede generar un enlace suelto.
        Assert.Equal(HttpStatusCode.Conflict, (await max.PostAsync($"/proyectos/{proyecto.Id}/enlace", null)).StatusCode);
        await EnviarAsync(max, proyecto.Id);
        Assert.Equal(HttpStatusCode.Conflict, (await max.PostAsync($"/proyectos/{proyecto.Id}/enviar", null)).StatusCode);
    }

    [Fact]
    public async Task Cancelar_revoca_el_enlace()
    {
        var max = await fixture.Api.NuevoAsync();
        var proyecto = await CrearAsync(max);
        var enlace = await EnviarAsync(max, proyecto.Id);

        await max.PostAsync($"/proyectos/{proyecto.Id}/cancelar", null);

        Assert.Equal(HttpStatusCode.NotFound, (await fixture.Api.Cliente().GetAsync($"/publico/{enlace.Token}")).StatusCode);
    }

    [Fact]
    public async Task Otro_usuario_no_puede_enviar_ni_revocar_proyectos_ajenos()
    {
        var ana = await fixture.Api.NuevoAsync();
        var beto = await fixture.Api.NuevoAsync();
        var deAna = await CrearAsync(ana);

        Assert.Equal(HttpStatusCode.NotFound, (await beto.PostAsync($"/proyectos/{deAna.Id}/enviar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await beto.PostAsync($"/proyectos/{deAna.Id}/enlace/revocar", null)).StatusCode);
        Assert.Equal("Borrador", (await ana.GetFromJsonAsync<ProyectoDto>($"/proyectos/{deAna.Id}", Json))!.Estado);
    }

    private static async Task<EnlaceCreadoDto> EnviarAsync(HttpClient api, Guid proyectoId)
    {
        var respuesta = await api.PostAsync($"/proyectos/{proyectoId}/enviar", null);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<EnlaceCreadoDto>(Json))!;
    }

    private static async Task<HttpClient> NuevoUsuarioAsync(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> api)
    {
        var cliente = api.CreateClient(new() { HandleCookies = false });
        var respuesta = await cliente.PostAsJsonAsync("/auth/registro",
            new { nombre = "Max", email = $"{Guid.NewGuid():N}@prueba.mx", contrasena = "contrasena-segura-123" });
        var token = (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString();
        cliente.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return cliente;
    }
}

// Reloj que la prueba adelanta a mano, para probar expiraciones sin esperar.
public sealed class RelojAjustable(DateTimeOffset inicio) : TimeProvider
{
    private DateTimeOffset _ahora = inicio;
    public override DateTimeOffset GetUtcNow() => _ahora;
    public void Avanzar(TimeSpan tiempo) => _ahora += tiempo;
}
