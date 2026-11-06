using Hito.Domain.Proyectos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hito.Infrastructure.Persistence.Configuraciones;

public sealed class EnlaceDeClienteConfiguracion : IEntityTypeConfiguration<EnlaceDeCliente>
{
    public void Configure(EntityTypeBuilder<EnlaceDeCliente> builder)
    {
        builder.ToTable("EnlacesDeCliente");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.HashDelToken).HasMaxLength(64).IsRequired();
        // Único: el enlace público se busca por el hash del token.
        builder.HasIndex(e => e.HashDelToken).IsUnique();
        builder.HasIndex(e => e.ProyectoId);
        builder.HasOne<Proyecto>().WithMany().HasForeignKey(e => e.ProyectoId).OnDelete(DeleteBehavior.Cascade);
    }
}
