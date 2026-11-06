using System.Security.Claims;
using Hito.Application.Comun;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Hito.Api.Auth;

// El id sale del token validado, nunca de la URL ni del cuerpo: así nadie puede hacerse pasar por otro.
public sealed class UsuarioActual(IHttpContextAccessor accesor) : IUsuarioActual
{
    public Guid? Id =>
        Guid.TryParse(accesor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;
}
