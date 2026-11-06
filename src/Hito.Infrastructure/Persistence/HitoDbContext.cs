using System.Linq.Expressions;
using Hito.Application.Comun;
using Hito.Domain.Clientes;
using Hito.Domain.Proyectos;
using Hito.Domain.Comun;
using Hito.Infrastructure.Identidad;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hito.Infrastructure.Persistence;

// IdentityUserContext: solo las tablas de usuarios; el alcance no tiene roles.
public sealed class HitoDbContext(DbContextOptions<HitoDbContext> options, IUsuarioActual usuarioActual)
    : IdentityUserContext<Usuario, Guid>(options)
{
    public DbSet<SesionDeRenovacion> Sesiones => Set<SesionDeRenovacion>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<EnlaceDeCliente> Enlaces => Set<EnlaceDeCliente>();

    // EF lee esta propiedad en cada consulta, así el filtro usa el usuario de la petición actual.
    // Sin usuario autenticado vale Guid.Empty y el filtro no devuelve nada.
    private Guid UsuarioActualId => usuarioActual.Id ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HitoDbContext).Assembly);
        AplicarFiltroPorDueno(modelBuilder);
    }

    private void AplicarFiltroPorDueno(ModelBuilder modelBuilder)
    {
        var propias = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(IPerteneceAUsuario).IsAssignableFrom(t.ClrType));

        foreach (var tipo in propias)
        {
            // e => e.OwnerId == this.UsuarioActualId
            var e = Expression.Parameter(tipo.ClrType, "e");
            var filtro = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(e, nameof(IPerteneceAUsuario.OwnerId)),
                    Expression.Property(Expression.Constant(this), nameof(UsuarioActualId))),
                e);
            tipo.SetQueryFilter(filtro);
        }
    }
}
