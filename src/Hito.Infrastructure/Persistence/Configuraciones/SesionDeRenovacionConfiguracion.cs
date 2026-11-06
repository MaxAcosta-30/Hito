using Hito.Infrastructure.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hito.Infrastructure.Persistence.Configuraciones;

public sealed class SesionDeRenovacionConfiguracion : IEntityTypeConfiguration<SesionDeRenovacion>
{
    public void Configure(EntityTypeBuilder<SesionDeRenovacion> builder)
    {
        builder.ToTable("SesionesDeRenovacion");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.HashDelToken).HasMaxLength(64).IsRequired();
        builder.HasIndex(s => s.HashDelToken).IsUnique();
        builder.HasIndex(s => s.UsuarioId);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(s => s.UsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}
