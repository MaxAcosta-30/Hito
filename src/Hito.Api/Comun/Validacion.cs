using System.Net.Mail;

namespace Hito.Api.Comun;

// Junta los errores por campo y responde 400 con ValidationProblem, igual que en /auth.
public sealed class Validacion
{
    private readonly Dictionary<string, List<string>> _errores = [];

    public bool HayErrores => _errores.Count > 0;

    public void Agregar(string campo, string mensaje)
    {
        if (!_errores.TryGetValue(campo, out var lista)) _errores[campo] = lista = [];
        lista.Add(mensaje);
    }

    // Texto obligatorio: devuelve el valor sin espacios a los lados.
    public string Requerido(string campo, string? valor, int maximo, string mensaje)
    {
        var limpio = valor?.Trim() ?? "";
        if (limpio.Length is 0 || limpio.Length > maximo) Agregar(campo, mensaje);
        return limpio;
    }

    // Texto opcional: vacío se guarda como null.
    public string? Opcional(string campo, string? valor, int maximo, string mensaje)
    {
        var limpio = valor?.Trim();
        if (string.IsNullOrEmpty(limpio)) return null;
        if (limpio.Length > maximo) Agregar(campo, mensaje);
        return limpio;
    }

    public string? EmailOpcional(string campo, string? valor)
    {
        var email = Opcional(campo, valor?.ToLowerInvariant(), 256, "El correo no es válido.");
        if (email is not null && !MailAddress.TryCreate(email, out _)) Agregar(campo, "El correo no es válido.");
        return email;
    }

    public IResult Problema() =>
        Results.ValidationProblem(_errores.ToDictionary(e => e.Key, e => e.Value.ToArray()));
}
