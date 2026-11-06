using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Hito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hito.IntegrationTests;

public sealed class DemoTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    // Crea una demo y devuelve un HttpClient con su sesión y el id del usuario.
    private static async Task<(HttpClient Api, Guid UsuarioId)> DemoAsync(WebApplicationFactory<Program> app)
    {
        var api = app.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        var respuesta = await api.PostAsync("/demo", null);
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var sesion = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(sesion.GetProperty("usuario").GetProperty("esDemo").GetBoolean());
        api.DefaultRequestHeaders.Authorization = new("Bearer", sesion.GetProperty("accessToken").GetString());
        return (api, sesion.GetProperty("usuario").GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Con_un_clic_crea_una_cuenta_con_datos_de_ejemplo_coherentes()
    {
        var (demo, _) = await DemoAsync(fixture.Api);

        var panel = await demo.GetFromJsonAsync<JsonElement>("/cobros");
        // Tienda: 12,000 + 20,000 + 4,000 pendientes; Citas: 15,000 + 25,000.
        Assert.Equal(76000m, panel.GetProperty("porCobrar").GetDecimal());
        Assert.Equal(12000m, panel.GetProperty("aprobadoSinPagar").GetDecimal());
        Assert.Equal(27000m, panel.GetProperty("vencido").GetDecimal());
        Assert.Equal(2, panel.GetProperty("proyectosActivos").GetInt32());

        var proyectos = (await demo.GetFromJsonAsync<List<JsonElement>>("/proyectos"))!;
        Assert.Equal(3, proyectos.Count);
        var tienda = proyectos.Single(p => p.GetProperty("nombre").GetString() == "Tienda en línea").GetProperty("id").GetGuid();
        var historial = (await demo.GetFromJsonAsync<List<JsonElement>>($"/proyectos/{tienda}/eventos"))!;
        Assert.True(historial.Count >= 9);
        Assert.Equal(3, (await demo.GetFromJsonAsync<List<JsonElement>>("/clientes"))!.Count);
    }

    [Fact]
    public async Task Cada_demo_esta_aislada_de_las_demas()
    {
        var (a, _) = await DemoAsync(fixture.Api);
        var (b, _) = await DemoAsync(fixture.Api);

        var deA = (await a.GetFromJsonAsync<List<JsonElement>>("/proyectos"))!.Select(p => p.GetProperty("id").GetGuid()).ToList();
        var deB = (await b.GetFromJsonAsync<List<JsonElement>>("/proyectos"))!.Select(p => p.GetProperty("id").GetGuid()).ToList();

        Assert.Empty(deA.Intersect(deB));
        Assert.Equal(HttpStatusCode.NotFound, (await b.GetAsync($"/proyectos/{deA[0]}")).StatusCode);
    }

    [Fact]
    public async Task Las_demos_vencidas_se_borran_con_todos_sus_datos_al_crear_otra()
    {
        var reloj = new RelojAjustable(DateTimeOffset.UtcNow);
        await using var app = fixture.Api.WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddSingleton<TimeProvider>(reloj)));
        var (vieja, viejaId) = await DemoAsync(app);
        // Que la demo vieja también tenga un enlace de cliente.
        var tienda = (await vieja.GetFromJsonAsync<List<JsonElement>>("/proyectos"))!
            .Single(p => p.GetProperty("nombre").GetString() == "Tienda en línea").GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.OK, (await vieja.PostAsync($"/proyectos/{tienda}/enlace", null)).StatusCode);

        reloj.Avanzar(TimeSpan.FromHours(25));
        await DemoAsync(app);

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HitoDbContext>();
        Assert.False(await db.Users.AnyAsync(u => u.Id == viejaId));
        Assert.False(await db.Proyectos.IgnoreQueryFilters().AnyAsync(p => p.OwnerId == viejaId));
        Assert.False(await db.Clientes.IgnoreQueryFilters().AnyAsync(c => c.OwnerId == viejaId));
        Assert.False(await db.Eventos.IgnoreQueryFilters().AnyAsync(e => e.OwnerId == viejaId));
        Assert.False(await db.Enlaces.IgnoreQueryFilters().AnyAsync(e => e.OwnerId == viejaId));
    }

    [Fact]
    public async Task Crear_demos_tiene_limite_por_minuto()
    {
        await using var app = fixture.Api.WithWebHostBuilder(b => b.UseSetting("Demo:PorMinuto", "2"));
        var api = app.CreateClient();

        var codigos = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++) codigos.Add((await api.PostAsync("/demo", null)).StatusCode);

        Assert.Equal([HttpStatusCode.Created, HttpStatusCode.Created, HttpStatusCode.TooManyRequests], codigos);
    }
}
