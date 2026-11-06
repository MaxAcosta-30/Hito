using System.Net;
using System.Net.Http.Json;
using static Hito.IntegrationTests.Proyectos;

namespace Hito.IntegrationTests;

public sealed class ProyectosTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    [Fact]
    public async Task Crear_un_proyecto_calcula_el_total_en_el_servidor()
    {
        var max = await fixture.Api.NuevoAsync();
        var clienteId = await CrearClienteAsync(max);

        // El total que mande el navegador se ignora: no existe en la solicitud.
        var respuesta = await max.PostAsJsonAsync("/proyectos", new
        {
            clienteId,
            nombre = "Tienda en línea",
            total = 1m,
            hitos = new[] { NuevoHito("Diseño", 8000m), NuevoHito("Landing", 12000.50m) },
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        var proyecto = await LeerAsync(respuesta);
        Assert.Equal(20000.50m, proyecto.Total);
        Assert.Equal("Borrador", proyecto.Estado);
        Assert.Equal("Panadería Rosales", proyecto.Cliente.Nombre);
        var version = Assert.Single(proyecto.Versiones);
        Assert.Equal(["Diseño", "Landing"], version.Hitos.Select(h => h.Titulo));
        Assert.All(version.Hitos, h => Assert.Equal("Pendiente", h.Estado));
    }

    [Fact]
    public async Task Datos_invalidos_responden_400_con_el_campo_de_cada_hito()
    {
        var max = await fixture.Api.NuevoAsync();
        var clienteId = await CrearClienteAsync(max);

        var respuesta = await max.PostAsJsonAsync("/proyectos", new
        {
            clienteId,
            nombre = "",
            hitos = new[] { NuevoHito("Diseño", 8000m), new { titulo = "", monto = -5m, fechaEntrega = (string?)null, entregable = "" } },
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("\"nombre\"", cuerpo);
        Assert.Contains("hitos[1].titulo", cuerpo);
        Assert.Contains("hitos[1].monto", cuerpo);
        Assert.Contains("hitos[1].fechaEntrega", cuerpo);
        Assert.Contains("hitos[1].entregable", cuerpo);
        Assert.DoesNotContain("hitos[0]", cuerpo);
    }

    [Fact]
    public async Task Editar_el_alcance_del_borrador_cambia_hitos_y_total()
    {
        var max = await fixture.Api.NuevoAsync();
        var proyecto = await CrearAsync(max);
        var diseno = proyecto.Versiones[0].Hitos[0];

        var respuesta = await max.PutAsJsonAsync($"/proyectos/{proyecto.Id}/alcance", new
        {
            nombre = "Tienda en línea v2",
            hitos = new[] { NuevoHito("Diseño de marca", 9000m, diseno.Id), NuevoHito("Capacitación", 4000m) },
        });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var editado = await LeerAsync(respuesta);
        Assert.Equal(13000m, editado.Total);
        Assert.Equal("Tienda en línea v2", editado.Nombre);
        Assert.Equal(diseno.Id, editado.Versiones[0].Hitos[0].Id);
        Assert.Equal(2, editado.Versiones[0].Hitos.Count);
        Assert.Single(editado.Versiones);
    }

    [Fact]
    public async Task Un_cliente_archivado_no_recibe_proyectos_nuevos()
    {
        var max = await fixture.Api.NuevoAsync();
        var clienteId = await CrearClienteAsync(max);
        await max.PostAsync($"/clientes/{clienteId}/archivar", null);

        var respuesta = await max.PostAsJsonAsync("/proyectos",
            new { clienteId, nombre = "X", hitos = new[] { NuevoHito("A", 1m) } });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("archivado", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Cancelar_deja_el_proyecto_en_solo_lectura_y_una_transicion_invalida_responde_409()
    {
        var max = await fixture.Api.NuevoAsync();
        var proyecto = await CrearAsync(max);

        var cancelar = await max.PostAsync($"/proyectos/{proyecto.Id}/cancelar", null);
        Assert.Equal("Cancelado", (await LeerAsync(cancelar)).Estado);

        var otraVez = await max.PostAsync($"/proyectos/{proyecto.Id}/cancelar", null);
        var editar = await max.PutAsJsonAsync($"/proyectos/{proyecto.Id}/alcance",
            new { nombre = "X", hitos = new[] { NuevoHito("A", 1m) } });

        Assert.Equal(HttpStatusCode.Conflict, otraVez.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, editar.StatusCode);
        Assert.Contains("cancelado", await editar.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Un_usuario_no_puede_ver_ni_tocar_proyectos_ni_clientes_de_otro()
    {
        var ana = await fixture.Api.NuevoAsync();
        var beto = await fixture.Api.NuevoAsync();
        var deAna = await CrearAsync(ana);

        var lista = await beto.GetFromJsonAsync<List<ProyectoDto>>("/proyectos", Json);
        Assert.DoesNotContain(lista!, p => p.Id == deAna.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await beto.GetAsync($"/proyectos/{deAna.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await beto.PutAsJsonAsync($"/proyectos/{deAna.Id}/alcance",
            new { nombre = "X", hitos = new[] { NuevoHito("A", 1m) } })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await beto.PostAsync($"/proyectos/{deAna.Id}/cancelar", null)).StatusCode);

        // Tampoco puede crear un proyecto con el cliente de Ana.
        var conClienteAjeno = await beto.PostAsJsonAsync("/proyectos",
            new { clienteId = deAna.Cliente.Id, nombre = "X", hitos = new[] { NuevoHito("A", 1m) } });
        Assert.Equal(HttpStatusCode.BadRequest, conClienteAjeno.StatusCode);

        Assert.Equal("Borrador", (await ana.GetFromJsonAsync<ProyectoDto>($"/proyectos/{deAna.Id}", Json))!.Estado);
    }
}
