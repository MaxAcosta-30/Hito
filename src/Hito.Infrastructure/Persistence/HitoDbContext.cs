using Microsoft.EntityFrameworkCore;

namespace Hito.Infrastructure.Persistence;

// Las entidades llegan en la Etapa 3. Por ahora el contexto sirve para el health check de /health/ready.
public sealed class HitoDbContext(DbContextOptions<HitoDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HitoDbContext).Assembly);
    }
}
