using System.Net;
using System.Net.Http.Json;
using static Hito.IntegrationTests.Proyectos;

namespace Hito.IntegrationTests;

public sealed class PagosTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    // Proyecto aceptado (Diseño $8,000 y Landing $12,000) con Diseño aprobado.
    private async Task<(HttpClient Max, HttpClient Cliente, ProyectoDto Proyecto, string Token)> ConDisenoAprobadoAsync()
    {
        var max = await fixture.Api.NuevoAsync();
        var cliente = fixture.Api.Cliente();
        var (proyecto, token) = await AceptadoAsync(max, cliente);
        var diseno = proyecto.Versiones[0].Hitos[0].Id;
        await max.PostAsJsonAsync($"/proyectos/{proyecto.Id}/hitos/{diseno}/entregar", new { evidenciaUrl = "https://marca.mx" });
        await cliente.PostAsync($"/publico/{token}/hitos/{diseno}/aprobar", null);
        return (max, cliente, proyecto, token);
    }

    private static Task<HttpResponseMessage> PagarAsync(HttpClient api, Guid proyectoId, Guid hitoId, decimal monto) =>
        api.PostAsJsonAsync($"/proyectos/{proyectoId}/hitos/{hitoId}/pagos", new { monto, fecha = "2026-11-06", nota = "Transferencia" });

    [Fact]
    public async Task Pagar_un_hito_aprobado_lo_marca_pagado_y_no_permite_pagar_de_mas()
    {
        var (max, _, proyecto, _) = await ConDisenoAprobadoAsync();
        var diseno = proyecto.Versiones[0].Hitos[0].Id;

        var demasiado = await PagarAsync(max, proyecto.Id, diseno, 8000.01m);
        Assert.Equal(HttpStatusCode.Conflict, demasiado.StatusCode);
        Assert.Contains("$8,000.00", await demasiado.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, (await PagarAsync(max, proyecto.Id, diseno, 3000m)).StatusCode);
        var pagado = await LeerAsync(await PagarAsync(max, proyecto.Id, diseno, 5000m));
        Assert.Equal("Pagado", pagado.Versiones[0].Hitos[0].Estado);
        Assert.Equal(HttpStatusCode.Conflict, (await PagarAsync(max, proyecto.Id, diseno, 1m)).StatusCode);
    }

    [Fact]
    public async Task Un_hito_sin_aprobar_no_se_paga()
    {
        var (max, _, proyecto, _) = await ConDisenoAprobadoAsync();
        var landing = proyecto.Versiones[0].Hitos[1].Id;

        var respuesta = await PagarAsync(max, proyecto.Id, landing, 100m);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("aprobado", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Pagar_todos_los_hitos_completa_el_proyecto()
    {
        var (max, cliente, proyecto, token) = await ConDisenoAprobadoAsync();
        var (diseno, landing) = (proyecto.Versiones[0].Hitos[0].Id, proyecto.Versiones[0].Hitos[1].Id);
        await PagarAsync(max, proyecto.Id, diseno, 8000m);
        await max.PostAsJsonAsync($"/proyectos/{proyecto.Id}/hitos/{landing}/entregar", new { evidenciaUrl = "https://landing.mx" });
        await cliente.PostAsync($"/publico/{token}/hitos/{landing}/aprobar", null);

        var completo = await LeerAsync(await PagarAsync(max, proyecto.Id, landing, 12000m));

        Assert.Equal("Completado", completo.Estado);
    }

    [Fact]
    public async Task Dos_pagos_simultaneos_no_pueden_pagar_de_mas()
    {
        var (max, _, proyecto, _) = await ConDisenoAprobadoAsync();
        var diseno = proyecto.Versiones[0].Hitos[0].Id;
        var otraPestana = fixture.Api.Cliente();
        otraPestana.DefaultRequestHeaders.Authorization = max.DefaultRequestHeaders.Authorization;

        // Dos pestañas registran el pago completo al mismo tiempo.
        var respuestas = await Task.WhenAll(
            PagarAsync(max, proyecto.Id, diseno, 8000m),
            PagarAsync(otraPestana, proyecto.Id, diseno, 8000m));

        Assert.Single(respuestas, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Single(respuestas, r => r.StatusCode == HttpStatusCode.Conflict);
        var final = await max.GetFromJsonAsync<ProyectoDto>($"/proyectos/{proyecto.Id}", Json);
        Assert.Equal(8000m, final!.Pagado);
    }

    [Fact]
    public async Task Datos_de_pago_incompletos_responden_400_y_otro_usuario_recibe_404()
    {
        var (max, _, proyecto, _) = await ConDisenoAprobadoAsync();
        var diseno = proyecto.Versiones[0].Hitos[0].Id;
        var beto = await fixture.Api.NuevoAsync();

        var incompleto = await max.PostAsJsonAsync($"/proyectos/{proyecto.Id}/hitos/{diseno}/pagos", new { nota = "x" });
        Assert.Equal(HttpStatusCode.BadRequest, incompleto.StatusCode);
        var cuerpo = await incompleto.Content.ReadAsStringAsync();
        Assert.Contains("\"monto\"", cuerpo);
        Assert.Contains("\"fecha\"", cuerpo);

        Assert.Equal(HttpStatusCode.NotFound, (await PagarAsync(beto, proyecto.Id, diseno, 100m)).StatusCode);
    }
}
