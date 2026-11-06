using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Hito.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static Hito.IntegrationTests.Proyectos;

namespace Hito.IntegrationTests;

public sealed class HistorialTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    [Fact]
    public async Task El_historial_registra_quien_que_y_cuando_de_cada_accion()
    {
        var max = await fixture.Api.NuevoAsync();
        var cliente = fixture.Api.Cliente();
        var (p, token) = await AceptadoAsync(max, cliente);
        var diseno = p.Versiones[0].Hitos[0].Id;
        await max.PostAsJsonAsync($"/proyectos/{p.Id}/hitos/{diseno}/entregar", new { evidenciaUrl = "https://marca.mx" });
        await cliente.PostAsync($"/publico/{token}/hitos/{diseno}/aprobar", null);
        await max.PostAsJsonAsync($"/proyectos/{p.Id}/hitos/{diseno}/pagos", new { monto = 8000m, fecha = "2026-11-06" });

        var eventos = (await max.GetFromJsonAsync<List<JsonElement>>($"/proyectos/{p.Id}/eventos"))!;

        Assert.Equal(["PagoRegistrado", "HitoAprobado", "HitoEntregado", "VersionAceptada", "VersionEnviada", "Creado"],
            eventos.Select(e => e.GetProperty("tipo").GetString()));
        Assert.Equal("Panadería Rosales", eventos[1].GetProperty("quien").GetString());
        Assert.Equal("Cliente", eventos[1].GetProperty("actor").GetString());
        Assert.Equal("Max", eventos[0].GetProperty("quien").GetString());
        Assert.Contains("$8,000.00", eventos[0].GetProperty("descripcion").GetString());
        Assert.True(eventos[0].GetProperty("fecha").GetDateTimeOffset() > DateTimeOffset.UtcNow.AddMinutes(-5));
    }

    [Fact]
    public async Task La_api_no_permite_editar_ni_borrar_eventos()
    {
        var max = await fixture.Api.NuevoAsync();
        var p = await CrearAsync(max);
        var ruta = $"/proyectos/{p.Id}/eventos";

        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await max.DeleteAsync(ruta)).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await max.PutAsJsonAsync(ruta, new { descripcion = "x" })).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await max.PatchAsJsonAsync(ruta, new { descripcion = "x" })).StatusCode);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await max.PostAsJsonAsync(ruta, new { descripcion = "x" })).StatusCode);
        Assert.Single((await max.GetFromJsonAsync<List<JsonElement>>(ruta))!);
    }

    [Fact]
    public async Task Ni_siquiera_el_codigo_del_servidor_puede_modificar_o_borrar_un_evento()
    {
        var max = await fixture.Api.NuevoAsync();
        var p = await CrearAsync(max);
        using var scope = fixture.Api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HitoDbContext>();
        var evento = await db.Eventos.IgnoreQueryFilters().SingleAsync(e => e.ProyectoId == p.Id);

        db.Entry(evento).Property(e => e.Descripcion).CurrentValue = "editado";
        Assert.Throws<InvalidOperationException>(() => db.SaveChanges());
        db.ChangeTracker.Clear();

        db.Eventos.Remove(await db.Eventos.IgnoreQueryFilters().SingleAsync(e => e.ProyectoId == p.Id));
        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Otro_usuario_no_ve_el_historial()
    {
        var ana = await fixture.Api.NuevoAsync();
        var beto = await fixture.Api.NuevoAsync();
        var p = await CrearAsync(ana);

        Assert.Equal(HttpStatusCode.NotFound, (await beto.GetAsync($"/proyectos/{p.Id}/eventos")).StatusCode);
    }
}
