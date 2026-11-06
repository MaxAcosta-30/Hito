using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Hito.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

public sealed class HealthTests(PostgresFixture db) : IClassFixture<PostgresFixture>
{
    // Puerto 1: nada escucha ahí, así simulamos una base caída sin esperar timeouts largos.
    private const string UnreachableDatabase =
        "Host=127.0.0.1;Port=1;Database=hito;Username=x;Password=x;Timeout=2";

    [Fact]
    public async Task Live_responde_200_aunque_la_base_no_este_disponible()
    {
        await using var factory = CreateFactory(UnreachableDatabase);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Ready_responde_200_con_postgres_real()
    {
        await using var factory = CreateFactory(db.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Ready_responde_503_si_la_base_no_esta_disponible()
    {
        await using var factory = CreateFactory(UnreachableDatabase);
        using var client = factory.CreateClient();

        var elapsed = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(10), $"Tardó {elapsed.Elapsed.TotalSeconds:F1} s");
    }

    private static WebApplicationFactory<Program> CreateFactory(string connectionString) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("ConnectionStrings:Hito", connectionString));
}
