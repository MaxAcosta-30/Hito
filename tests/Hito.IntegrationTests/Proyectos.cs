using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hito.IntegrationTests;

public sealed record HitoDto(
    Guid Id, int Orden, string Titulo, decimal Monto, DateOnly FechaEntrega, string Entregable, string Estado,
    string? EvidenciaUrl = null, string? ComentarioCambios = null);
public sealed record VersionDto(int Numero, string Estado, DateTimeOffset? EnviadaEn, DateTimeOffset? AceptadaEn, decimal Total, List<HitoDto> Hitos);
public sealed record ClienteResumenDto(Guid Id, string Nombre);
public sealed record ProyectoDto(
    Guid Id, string Nombre, ClienteResumenDto Cliente, string Estado, decimal Total, int VersionActual, int UltimaVersion, List<VersionDto> Versiones);

// Ayudas compartidas por las pruebas de proyectos, enlaces y aceptación.
public static class Proyectos
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public static object NuevoHito(string titulo, decimal monto, Guid? id = null) =>
        new { id, titulo, monto, fechaEntrega = "2026-12-01", entregable = $"Entrega de {titulo}" };

    public static async Task<Guid> CrearClienteAsync(HttpClient api, string nombre = "Panadería Rosales")
    {
        var respuesta = await api.PostAsJsonAsync("/clientes", new { nombre });
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
    }

    public static async Task<ProyectoDto> CrearAsync(HttpClient api, params object[] hitos)
    {
        var clienteId = await CrearClienteAsync(api);
        var respuesta = await api.PostAsJsonAsync("/proyectos", new
        {
            clienteId,
            nombre = "Tienda en línea",
            hitos = hitos.Length > 0 ? hitos : [NuevoHito("Diseño", 8000m), NuevoHito("Landing", 12000m)],
        });
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<ProyectoDto>(Json))!;
    }

    // Crea un proyecto, lo envía y el cliente lo acepta. Devuelve el proyecto aceptado y el token del cliente.
    public static async Task<(ProyectoDto Proyecto, string Token)> AceptadoAsync(HttpClient api, HttpClient anonimo, params object[] hitos)
    {
        var proyecto = await CrearAsync(api, hitos);
        var enviar = await api.PostAsync($"/proyectos/{proyecto.Id}/enviar", null);
        var token = (await enviar.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString()!;
        var aceptar = await anonimo.PostAsJsonAsync($"/publico/{token}/aceptar", new { version = 1 });
        Assert.Equal(System.Net.HttpStatusCode.OK, aceptar.StatusCode);
        return ((await api.GetFromJsonAsync<ProyectoDto>($"/proyectos/{proyecto.Id}", Json))!, token);
    }

    public static async Task<ProyectoDto> LeerAsync(HttpResponseMessage respuesta) =>
        (await respuesta.Content.ReadFromJsonAsync<ProyectoDto>(Json))!;
}
