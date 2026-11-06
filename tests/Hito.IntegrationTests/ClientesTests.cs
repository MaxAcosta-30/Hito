using System.Net;
using System.Net.Http.Json;

namespace Hito.IntegrationTests;

public sealed class ClientesTests(ApiConBase fixture) : IClassFixture<ApiConBase>
{
    private sealed record ClienteDto(
        Guid Id, string Nombre, string? Empresa, string? Email, string? Telefono, DateTimeOffset CreadoEn, DateTimeOffset? ArchivadoEn);

    [Fact]
    public async Task Crear_editar_listar_y_archivar_un_cliente()
    {
        var max = await fixture.Api.NuevoAsync();

        var creado = await CrearAsync(max, new { nombre = "  Panadería Rosales ", email = "Ana@Rosales.MX", telefono = "+52 667 123 4567" });
        Assert.Equal("Panadería Rosales", creado.Nombre);
        Assert.Equal("ana@rosales.mx", creado.Email);
        Assert.Null(creado.Empresa);

        var editar = await max.PutAsJsonAsync($"/clientes/{creado.Id}", new { nombre = "Panadería Rosales", empresa = "Rosales SA de CV" });
        Assert.Equal(HttpStatusCode.OK, editar.StatusCode);
        Assert.Equal("Rosales SA de CV", (await editar.Content.ReadFromJsonAsync<ClienteDto>())!.Empresa);

        Assert.Contains(await ListarAsync(max), c => c.Id == creado.Id);

        var archivar = await max.PostAsync($"/clientes/{creado.Id}/archivar", null);
        Assert.Equal(HttpStatusCode.OK, archivar.StatusCode);
        Assert.DoesNotContain(await ListarAsync(max), c => c.Id == creado.Id);
        Assert.Contains(await ListarAsync(max, archivados: true), c => c.Id == creado.Id);

        await max.PostAsync($"/clientes/{creado.Id}/restaurar", null);
        Assert.Contains(await ListarAsync(max), c => c.Id == creado.Id);
    }

    [Fact]
    public async Task Datos_invalidos_responden_400_con_el_error_en_su_campo()
    {
        var max = await fixture.Api.NuevoAsync();

        var respuesta = await max.PostAsJsonAsync("/clientes",
            new { nombre = "   ", email = "no-es-correo", telefono = "llámame", empresa = new string('x', 121) });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("\"nombre\"", cuerpo);
        Assert.Contains("\"email\"", cuerpo);
        Assert.Contains("\"telefono\"", cuerpo);
        Assert.Contains("\"empresa\"", cuerpo);
    }

    [Fact]
    public async Task Sin_token_responde_401()
    {
        var anonimo = fixture.Api.Cliente();

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonimo.GetAsync("/clientes")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonimo.PostAsJsonAsync("/clientes", new { nombre = "X" })).StatusCode);
    }

    // Criterio del caso 1: un usuario no puede leer ni modificar datos de otro.
    [Fact]
    public async Task Un_usuario_no_puede_leer_ni_modificar_los_clientes_de_otro()
    {
        var ana = await fixture.Api.NuevoAsync();
        var beto = await fixture.Api.NuevoAsync();
        var deAna = await CrearAsync(ana, new { nombre = "Cliente de Ana" });

        Assert.DoesNotContain(await ListarAsync(beto), c => c.Id == deAna.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await beto.GetAsync($"/clientes/{deAna.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await beto.PutAsJsonAsync($"/clientes/{deAna.Id}", new { nombre = "Robado" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await beto.PostAsync($"/clientes/{deAna.Id}/archivar", null)).StatusCode);

        // Y los datos de Ana quedan intactos.
        var deAnaDespues = await ana.GetFromJsonAsync<ClienteDto>($"/clientes/{deAna.Id}");
        Assert.Equal("Cliente de Ana", deAnaDespues!.Nombre);
        Assert.Null(deAnaDespues.ArchivadoEn);
    }

    private static async Task<ClienteDto> CrearAsync(HttpClient cliente, object datos)
    {
        var respuesta = await cliente.PostAsJsonAsync("/clientes", datos);
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<ClienteDto>())!;
    }

    private static async Task<List<ClienteDto>> ListarAsync(HttpClient cliente, bool archivados = false) =>
        (await cliente.GetFromJsonAsync<List<ClienteDto>>($"/clientes?archivados={archivados}"))!;
}
