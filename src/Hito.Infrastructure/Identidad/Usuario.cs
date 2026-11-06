using Microsoft.AspNetCore.Identity;

namespace Hito.Infrastructure.Identidad;

public sealed class Usuario : IdentityUser<Guid>
{
    public required string Nombre { get; set; }

    // Solo las cuentas de la demo: después de esta fecha se borran con todos sus datos.
    public DateTimeOffset? DemoExpiraEn { get; set; }
}
