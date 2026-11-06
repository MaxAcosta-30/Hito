using Hito.Infrastructure.Identidad;
using Hito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hito.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // EnableRetryOnFailure: tolera cortes breves del pooler de Supabase y los arranques en frío.
        services.AddDbContext<HitoDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

        services.AddIdentityCore<Usuario>(options =>
            {
                // NIST 800-63B: longitud mínima en lugar de reglas de símbolos.
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;
                // Límite de intentos de inicio de sesión: 5 fallos bloquean la cuenta 5 minutos.
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<HitoDbContext>()
            .AddErrorDescriber<ErroresDeIdentidadEnEspanol>();

        return services;
    }
}
