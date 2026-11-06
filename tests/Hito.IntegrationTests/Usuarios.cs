using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Hito.IntegrationTests;

public static class Usuarios
{
    // Registra un usuario nuevo y devuelve un HttpClient que ya manda su access token.
    public static async Task<HttpClient> NuevoAsync(this HitoApi api)
    {
        var cliente = api.Cliente();
        var respuesta = await cliente.PostAsJsonAsync("/auth/registro",
            new { nombre = "Max", email = $"{Guid.NewGuid():N}@prueba.mx", contrasena = "contrasena-segura-123" });
        respuesta.EnsureSuccessStatusCode();
        var token = (await respuesta.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
