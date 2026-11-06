using Hito.Domain.Proyectos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hito.Infrastructure.Persistence.Configuraciones;

public sealed class EventoDeProyectoConfiguracion : IEntityTypeConfiguration<EventoDeProyecto>
{
    public const string Secuencia = "Secuencia";

    public void Configure(EntityTypeBuilder<EventoDeProyecto> builder)
    {
        builder.ToTable("EventosDeProyecto");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Actor).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Descripcion).HasMaxLength(EventoDeProyecto.MaximoDescripcion).IsRequired();
        // Orden de llegada: un pago y "proyecto completado" pueden tener la misma fecha.
        builder.Property<long>(Secuencia).UseIdentityAlwaysColumn();
        builder.HasIndex(nameof(EventoDeProyecto.ProyectoId), Secuencia);
        builder.HasOne<Proyecto>().WithMany().HasForeignKey(e => e.ProyectoId).OnDelete(DeleteBehavior.Cascade);
    }
}
