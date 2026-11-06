using Microsoft.AspNetCore.Identity;

namespace Hito.Infrastructure.Identidad;

public sealed class Usuario : IdentityUser<Guid>
{
    public required string Nombre { get; set; }
}
