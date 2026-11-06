using Hito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Hito.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

// La API real contra la base indicada, con una clave JWT de pruebas.
public sealed class HitoApi(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Hito", connectionString);
        builder.UseSetting("Jwt:Clave", "clave-solo-para-pruebas-de-integracion-0123456789");
        builder.UseSetting("Auth:RegistrosPorMinuto", "1000");
    }

    // Sin manejo automático de cookies: las pruebas leen y mandan la cookie de renovación a mano.
    public HttpClient Cliente() => CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
}

// PostgreSQL con las migraciones aplicadas, compartido por las pruebas de una clase.
public sealed class ApiConBase : IAsyncLifetime
{
    private readonly PostgresFixture _postgres = new();

    public HitoApi Api { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();
        Api = new HitoApi(_postgres.ConnectionString);
        using var scope = Api.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<HitoDbContext>().Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Api.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
