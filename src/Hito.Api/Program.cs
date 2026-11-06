using Hito.Infrastructure;
using Hito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Hito")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:Hito'.");

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<HitoDbContext>("database", tags: ["ready"]);

var app = builder.Build();

// live: el proceso responde. No toca la base, así un fallo de SQL no provoca reinicios del contenedor.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// ready: el proceso puede atender peticiones reales, incluida la base de datos.
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.Run();
