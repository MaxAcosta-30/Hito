using Hito.Infrastructure.Identidad;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Hito.Api.Auth;

public sealed record OpcionesJwt(string Emisor, string Audiencia, SymmetricSecurityKey Llave)
{
    public static readonly TimeSpan Duracion = TimeSpan.FromMinutes(15);
}

public sealed class EmisorDeTokens(OpcionesJwt opciones, TimeProvider reloj)
{
    private readonly JsonWebTokenHandler _handler = new();

    public string Emitir(Usuario usuario)
    {
        var ahora = reloj.GetUtcNow().UtcDateTime;
        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = opciones.Emisor,
            Audience = opciones.Audiencia,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = usuario.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = usuario.Email!,
                [JwtRegisteredClaimNames.Name] = usuario.Nombre,
            },
            IssuedAt = ahora,
            NotBefore = ahora,
            Expires = ahora + OpcionesJwt.Duracion,
            SigningCredentials = new SigningCredentials(opciones.Llave, SecurityAlgorithms.HmacSha256),
        });
    }
}
