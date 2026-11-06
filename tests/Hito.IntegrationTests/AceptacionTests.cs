using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static Hito.IntegrationTests.Proyectos;

namespace Hito.IntegrationTests;

public sealed class AceptacionTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    [Fact]
    public async Task El_cliente_acepta_y_un_cambio_posterior_crea_una_version_que_debe_aceptarse()
    {
        var max = await fixture.Api.NuevoAsync();
        var proyecto = await CrearAsync(max);
        var token = await EnviarAsync(max, proyecto.Id);
        var cliente = fixture.Api.Cliente();

        var aceptar = await cliente.PostAsJsonAsync($"/publico/{token}/aceptar", new { version = 1 });
        Assert.Equal(HttpStatusCode.OK, aceptar.StatusCode);
        var aceptado = await max.GetFromJsonAsync<ProyectoDto>($"/proyectos/{proyecto.Id}", Json);
        Assert.Equal("Aceptado", aceptado!.Estado);

        // Congelado: editar el alcance aceptado responde 409.
        var editar = await max.PutAsJsonAsync($"/proyectos/{proyecto.Id}/alcance",
            new { nombre = "Tienda", hitos = new[] { NuevoHito("Diseño", 1m) } });
        Assert.Equal(HttpStatusCode.Conflict, editar.StatusCode);

        // Cambio posterior: versión 2 con un monto distinto.
        var v2 = await LeerAsync(await max.PostAsync($"/proyectos/{proyecto.Id}/versiones", null));
        Assert.Equal(2, v2.UltimaVersion);
        var diseno = v2.Versiones.Single(v => v.Numero == 2).Hitos[0];
        await max.PutAsJsonAsync($"/proyectos/{proyecto.Id}/alcance",
            new { nombre = "Tienda en línea", hitos = new[] { NuevoHito("Diseño", 9500m, diseno.Id) } });
        var token2 = await EnviarAsync(max, proyecto.Id);

        // Mientras tanto rige la versión 1, con sus montos originales.
        var antes = await max.GetFromJsonAsync<ProyectoDto>($"/proyectos/{proyecto.Id}", Json);
        Assert.Equal(1, antes!.VersionActual);
        Assert.Equal(20000m, antes.Total);
        var vista = await cliente.GetFromJsonAsync<JsonElement>($"/publico/{token2}");
        Assert.Equal(1, vista.GetProperty("vigente").GetProperty("numero").GetInt32());
        Assert.Equal(9500m, vista.GetProperty("porAceptar").GetProperty("total").GetDecimal());

        Assert.Equal(HttpStatusCode.OK, (await cliente.PostAsJsonAsync($"/publico/{token2}/aceptar", new { version = 2 })).StatusCode);
        var despues = await max.GetFromJsonAsync<ProyectoDto>($"/proyectos/{proyecto.Id}", Json);
        Assert.Equal(2, despues!.VersionActual);
        Assert.Equal(9500m, despues.Total);
        // La versión 1 quedó como estaba.
        Assert.Equal(20000m, despues.Versiones.Single(v => v.Numero == 1).Total);
    }

    [Fact]
    public async Task Aceptar_una_version_vieja_o_dos_veces_responde_409()
    {
        var max = await fixture.Api.NuevoAsync();
        var proyecto = await CrearAsync(max);
        var token = await EnviarAsync(max, proyecto.Id);
        var cliente = fixture.Api.Cliente();

        // El freelancer cambió el alcance mientras el cliente lo leía: la versión 1 ya no se puede aceptar.
        await max.PostAsync($"/proyectos/{proyecto.Id}/versiones", null);
        var vieja = await cliente.PostAsJsonAsync($"/publico/{token}/aceptar", new { version = 1 });
        Assert.Equal(HttpStatusCode.Conflict, vieja.StatusCode);

        var token2 = await EnviarAsync(max, proyecto.Id);
        Assert.Equal(HttpStatusCode.OK, (await cliente.PostAsJsonAsync($"/publico/{token2}/aceptar", new { version = 2 })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await cliente.PostAsJsonAsync($"/publico/{token2}/aceptar", new { version = 2 })).StatusCode);
    }

    [Fact]
    public async Task Sin_un_enlace_valido_no_se_acepta_nada()
    {
        var max = await fixture.Api.NuevoAsync();
        var proyecto = await CrearAsync(max);
        var token = await EnviarAsync(max, proyecto.Id);
        await max.PostAsync($"/proyectos/{proyecto.Id}/enlace/revocar", null);

        var respuesta = await fixture.Api.Cliente().PostAsJsonAsync($"/publico/{token}/aceptar", new { version = 1 });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("Enviado", (await max.GetFromJsonAsync<ProyectoDto>($"/proyectos/{proyecto.Id}", Json))!.Estado);
    }

    private static async Task<string> EnviarAsync(HttpClient api, Guid proyectoId)
    {
        var respuesta = await api.PostAsync($"/proyectos/{proyectoId}/enviar", null);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;
    }
}
