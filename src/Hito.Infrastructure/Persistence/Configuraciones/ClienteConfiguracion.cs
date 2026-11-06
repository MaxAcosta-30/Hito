using Hito.Domain.Clientes;
using Hito.Infrastructure.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hito.Infrastructure.Persistence.Configuraciones;

public sealed class ClienteConfiguracion : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).HasMaxLength(120).IsRequired();
        builder.Property(c => c.Empresa).HasMaxLength(120);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Telefono).HasMaxLength(30);
        builder.Ignore(c => c.Archivado);
        builder.HasIndex(c => c.OwnerId);
        // Borrar un usuario (las cuentas de la demo) borra sus clientes.
        builder.HasOne<Usuario>().WithMany().HasForeignKey(c => c.OwnerId).OnDelete(DeleteBehavior.Cascade);
    }
}
