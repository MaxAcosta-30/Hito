using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static Hito.IntegrationTests.Proyectos;

namespace Hito.IntegrationTests;

public sealed class EntregasTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    private const string Url = "https://tienda.rosales.mx";

    [Fact]
    public async Task Entregar_pedir_cambios_reentregar_y_aprobar()
    {
        var max = await fixture.Api.NuevoAsync();
        var cliente = fixture.Api.Cliente();
        var (proyecto, token) = await AceptadoAsync(max, cliente);
        var hito = proyecto.Versiones[0].Hitos[0];

        var entregado = await LeerAsync(await max.PostAsJsonAsync($"/proyectos/{proyecto.Id}/hitos/{hito.Id}/entregar", new { evidenciaUrl = Url }));
        Assert.Equal("Entregado", entregado.Versiones[0].Hitos[0].Estado);
        Assert.Equal(Url, entregado.Versiones[0].Hitos[0].EvidenciaUrl);

        // El cliente ve la evidencia y pide cambios con comentario.
        var vista = await cliente.GetFromJsonAsync<JsonElement>($"/publico/{token}");
        Assert.Equal(Url, vista.GetProperty("vigente").GetProperty("hitos")[0].GetProperty("evidenciaUrl").GetString());
        var cambios = await cliente.PostAsJsonAsync($"/publico/{token}/hitos/{hito.Id}/cambios", new { comentario = "El logo en azul" });
        Assert.Equal(HttpStatusCode.OK, cambios.StatusCode);
        var conCambios = await max.GetFromJsonAsync<ProyectoDto>($"/proyectos/{proyecto.Id}", Json);
        Assert.Equal("CambiosSolicitados", conCambios!.Versiones[0].Hitos[0].Estado);
        Assert.Equal("El logo en azul", conCambios.Versiones[0].Hitos[0].ComentarioCambios);

        await max.PostAsJsonAsync($"/proyectos/{proyecto.Id}/hitos/{hito.Id}/entregar", new { evidenciaUrl = Url + "/v2" });
        var aprobar = await cliente.PostAsync($"/publico/{token}/hitos/{hito.Id}/aprobar", null);
        Assert.Equal(HttpStatusCode.OK, aprobar.StatusCode);
        var aprobado = await max.GetFromJsonAsync<ProyectoDto>($"/proyectos/{proyecto.Id}", Json);
        Assert.Equal("Aprobado", aprobado!.Versiones[0].Hitos[0].Estado);
    }

    [Fact]
    public async Task Evidencia_que_no_es_http_y_cambios_sin_comentario_responden_400()
    {
        var max = await fixture.Api.NuevoAsync();
        var cliente = fixture.Api.Cliente();
        var (proyecto, token) = await AceptadoAsync(max, cliente);
        var hito = proyecto.Versiones[0].Hitos[0];

        var js = await max.PostAsJsonAsync($"/proyectos/{proyecto.Id}/hitos/{hito.Id}/entregar", new { evidenciaUrl = "javascript:alert(1)" });
        Assert.Equal(HttpStatusCode.BadRequest, js.StatusCode);
        Assert.Contains("evidenciaUrl", await js.Content.ReadAsStringAsync());

        await max.PostAsJsonAsync($"/proyectos/{proyecto.Id}/hitos/{hito.Id}/entregar", new { evidenciaUrl = Url });
        var sinComentario = await cliente.PostAsJsonAsync($"/publico/{token}/hitos/{hito.Id}/cambios", new { comentario = "  " });
        Assert.Equal(HttpStatusCode.BadRequest, sinComentario.StatusCode);
        Assert.Contains("comentario", await sinComentario.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Una_transicion_invalida_responde_409()
    {
        var max = await fixture.Api.NuevoAsync();
        var cliente = fixture.Api.Cliente();
        var (proyecto, token) = await AceptadoAsync(max, cliente);
        var hito = proyecto.Versiones[0].Hitos[0];

        // Aprobar un hito que no se ha entregado.
        var aprobar = await cliente.PostAsync($"/publico/{token}/hitos/{hito.Id}/aprobar", null);

        Assert.Equal(HttpStatusCode.Conflict, aprobar.StatusCode);
        Assert.Contains("pendiente", await aprobar.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Ni_otro_usuario_ni_el_enlace_de_otro_proyecto_pueden_mover_un_hito()
    {
        var ana = await fixture.Api.NuevoAsync();
        var beto = await fixture.Api.NuevoAsync();
        var cliente = fixture.Api.Cliente();
        var (deAna, _) = await AceptadoAsync(ana, cliente);
        var (_, tokenDeBeto) = await AceptadoAsync(beto, cliente);
        var hito = deAna.Versiones[0].Hitos[0];

        var entregar = await beto.PostAsJsonAsync($"/proyectos/{deAna.Id}/hitos/{hito.Id}/entregar", new { evidenciaUrl = Url });
        Assert.Equal(HttpStatusCode.NotFound, entregar.StatusCode);

        await ana.PostAsJsonAsync($"/proyectos/{deAna.Id}/hitos/{hito.Id}/entregar", new { evidenciaUrl = Url });
        // El cliente de Beto intenta aprobar un hito de Ana con su propio enlace.
        var aprobar = await cliente.PostAsync($"/publico/{tokenDeBeto}/hitos/{hito.Id}/aprobar", null);
        Assert.Equal(HttpStatusCode.Conflict, aprobar.StatusCode);
        Assert.Equal("Entregado", (await ana.GetFromJsonAsync<ProyectoDto>($"/proyectos/{deAna.Id}", Json))!.Versiones[0].Hitos[0].Estado);
    }
}
