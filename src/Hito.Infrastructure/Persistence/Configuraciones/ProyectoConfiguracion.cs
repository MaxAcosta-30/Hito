using Hito.Domain.Clientes;
using Hito.Domain.Proyectos;
using Hito.Infrastructure.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hito.Infrastructure.Persistence.Configuraciones;

public sealed class ProyectoConfiguracion : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("Proyectos");
        builder.HasKey(p => p.Id);
        // Los Id los genera el dominio. Sin esto, EF cree que un hito nuevo agregado a la colección ya existe y hace UPDATE.
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Nombre).HasMaxLength(Proyecto.MaximoNombre).IsRequired();
        builder.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
        // xmin de PostgreSQL como token de concurrencia: dos cambios simultáneos al mismo proyecto no se pisan.
        builder.Property(p => p.VersionDeFila).IsRowVersion();
        builder.Ignore(p => p.UltimaVersion);
        builder.Ignore(p => p.VersionVigente);
        builder.Ignore(p => p.VersionActual);
        builder.Ignore(p => p.Total);
        builder.Ignore(p => p.Pagado);
        builder.HasIndex(p => p.OwnerId);

        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Cascade);
        // Un cliente con proyectos no se borra: se archiva. NO ACTION (y no RESTRICT) se revisa al final de la sentencia,
        // así borrar un usuario de la demo puede borrar en cascada sus clientes y proyectos a la vez.
        builder.HasOne<Cliente>().WithMany().HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(p => p.Versiones).WithOne().HasForeignKey(v => v.ProyectoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Hitos).WithOne().HasForeignKey(h => h.ProyectoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Pagos).WithOne().HasForeignKey(p => p.ProyectoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PagoConfiguracion : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pagos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Monto).HasPrecision(18, 2);
        builder.Property(p => p.Nota).HasMaxLength(Pago.MaximoNota);
        builder.HasIndex(p => p.HitoId);
        // Un pago siempre apunta a un hito existente (NO ACTION, igual que los términos).
        builder.HasOne<HitoDeProyecto>().WithMany().HasForeignKey(p => p.HitoId).OnDelete(DeleteBehavior.ClientCascade);
    }
}

public sealed class VersionDeProyectoConfiguracion : IEntityTypeConfiguration<VersionDeProyecto>
{
    public void Configure(EntityTypeBuilder<VersionDeProyecto> builder)
    {
        builder.ToTable("VersionesDeProyecto");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();
        builder.Property(v => v.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Ignore(v => v.Total);
        builder.HasIndex(v => new { v.ProyectoId, v.Numero }).IsUnique();
        builder.HasMany(v => v.Terminos).WithOne().HasForeignKey(t => t.VersionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class HitoDeProyectoConfiguracion : IEntityTypeConfiguration<HitoDeProyecto>
{
    public void Configure(EntityTypeBuilder<HitoDeProyecto> builder)
    {
        builder.ToTable("Hitos");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();
        builder.Property(h => h.Estado).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.EvidenciaUrl).HasMaxLength(HitoDeProyecto.MaximoUrl);
        builder.Property(h => h.ComentarioCambios).HasMaxLength(HitoDeProyecto.MaximoComentario);
    }
}

public sealed class TerminosDeHitoConfiguracion : IEntityTypeConfiguration<TerminosDeHito>
{
    public void Configure(EntityTypeBuilder<TerminosDeHito> builder)
    {
        builder.ToTable("TerminosDeHito");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Titulo).HasMaxLength(DatosDeHito.MaximoTitulo).IsRequired();
        builder.Property(t => t.Entregable).HasMaxLength(DatosDeHito.MaximoEntregable).IsRequired();
        // Dinero: numeric(18,2), nunca float.
        builder.Property(t => t.Monto).HasPrecision(18, 2);
        builder.HasIndex(t => new { t.VersionId, t.HitoId }).IsUnique();
        // En la base es NO ACTION (por la misma razón que en Proyectos). ClientCascade solo le dice a EF que, si borra
        // un hito que tiene cargado (uno quitado de un borrador), borre también sus términos en memoria.
        builder.HasOne<HitoDeProyecto>().WithMany().HasForeignKey(t => t.HitoId).OnDelete(DeleteBehavior.ClientCascade);
    }
}
