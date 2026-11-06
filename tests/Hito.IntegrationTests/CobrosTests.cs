using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static Hito.IntegrationTests.Proyectos;

namespace Hito.IntegrationTests;

public sealed class CobrosTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    private static object HitoConFecha(string titulo, decimal monto, DateOnly fecha) =>
        new { titulo, monto, fechaEntrega = fecha.ToString("yyyy-MM-dd"), entregable = "Entrega" };

    [Fact]
    public async Task Los_totales_del_panel_coinciden_con_los_datos_registrados()
    {
        var max = await fixture.Api.NuevoAsync();
        var cliente = fixture.Api.Cliente();
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var (p, token) = await AceptadoAsync(max, cliente,
            HitoConFecha("Atrasado", 4000m, hoy.AddDays(-10)),
            HitoConFecha("Aprobado", 12000m, hoy.AddDays(10)),
            HitoConFecha("Futuro", 20000m, hoy.AddDays(30)));
        var aprobado = p.Versiones[0].Hitos.Single(h => h.Titulo == "Aprobado").Id;
        await max.PostAsJsonAsync($"/proyectos/{p.Id}/hitos/{aprobado}/entregar", new { evidenciaUrl = "https://x.mx" });
        await cliente.PostAsync($"/publico/{token}/hitos/{aprobado}/aprobar", null);
        await max.PostAsJsonAsync($"/proyectos/{p.Id}/hitos/{aprobado}/pagos", new { monto = 2000m, fecha = hoy.ToString("yyyy-MM-dd") });
        // Un borrador no cuenta.
        await CrearAsync(max, HitoConFecha("Borrador", 99999m, hoy));

        var panel = await max.GetFromJsonAsync<JsonElement>("/cobros");

        Assert.Equal(4000m + 10000m + 20000m, panel.GetProperty("porCobrar").GetDecimal());
        Assert.Equal(10000m, panel.GetProperty("aprobadoSinPagar").GetDecimal());
        Assert.Equal(4000m, panel.GetProperty("vencido").GetDecimal());
        Assert.Equal(1, panel.GetProperty("proyectosActivos").GetInt32());
        var atencion = panel.GetProperty("atencion").EnumerateArray().ToList();
        Assert.Equal(["Atrasado", "Aprobado"], atencion.Select(a => a.GetProperty("titulo").GetString()));
        Assert.Equal("Vencido", atencion[0].GetProperty("motivo").GetString());
        Assert.Equal("Panadería Rosales", atencion[0].GetProperty("cliente").GetString());
    }

    [Fact]
    public async Task El_panel_solo_suma_los_proyectos_del_usuario()
    {
        var ana = await fixture.Api.NuevoAsync();
        await AceptadoAsync(ana, fixture.Api.Cliente());
        var beto = await fixture.Api.NuevoAsync();

        var panel = await beto.GetFromJsonAsync<JsonElement>("/cobros");

        Assert.Equal(0m, panel.GetProperty("porCobrar").GetDecimal());
        Assert.Empty(panel.GetProperty("atencion").EnumerateArray());
        Assert.Equal(HttpStatusCode.Unauthorized, (await fixture.Api.Cliente().GetAsync("/cobros")).StatusCode);
    }
}
