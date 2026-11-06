using Hito.Infrastructure.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hito.Infrastructure.Persistence.Configuraciones;

public sealed class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
    }
}
