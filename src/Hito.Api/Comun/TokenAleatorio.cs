using System.Security.Cryptography;
using System.Text;

namespace Hito.Api.Comun;

// Tokens de 256 bits (refresh token y enlace del cliente). En la base solo se guarda su SHA-256:
// basta un hash rápido porque el token es aleatorio y no se puede adivinar, a diferencia de una contraseña.
public static class TokenAleatorio
{
    public static string Generar() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string Hash(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
