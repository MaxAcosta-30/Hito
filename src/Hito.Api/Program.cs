using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Hito.Api.Auth;
using Hito.Api.Clientes;
using Hito.Api.Cobros;
using Hito.Api.Comun;
using Hito.Api.Demo;
using Hito.Api.Proyectos;
using Hito.Api.Publico;
using System.Text.Json.Serialization;
using Hito.Application.Comun;
using Hito.Infrastructure;
using Hito.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Hito")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:Hito'.");

// HS256 necesita una clave de al menos 256 bits. Viene de una variable de entorno, nunca del repositorio.
var claveJwt = builder.Configuration["Jwt:Clave"];
if (string.IsNullOrEmpty(claveJwt) || Encoding.UTF8.GetByteCount(claveJwt) < 32)
    throw new InvalidOperationException("Falta 'Jwt:Clave' o tiene menos de 32 bytes.");
var opcionesJwt = new OpcionesJwt("hito-api", "hito-web", new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)));

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(opcionesJwt);
builder.Services.AddSingleton<EmisorDeTokens>();
builder.Services.AddScoped<Sesiones>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorDeReglas>();
// Los estados viajan como texto ("Borrador"), no como números: la API se lee sin conocer los enums.
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = opcionesJwt.Emisor,
            ValidAudience = opcionesJwt.Audiencia,
            IssuerSigningKey = opcionesJwt.Llave,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization();

// Límite global de registros por minuto: frena la creación masiva de cuentas.
var registrosPorMinuto = builder.Configuration.GetValue("Auth:RegistrosPorMinuto", 10);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(AuthEndpoints.PoliticaRegistro, limite =>
    {
        limite.PermitLimit = registrosPorMinuto;
        limite.Window = TimeSpan.FromMinutes(1);
        limite.QueueLimit = 0;
    });
    // Límite por IP al entrar: el bloqueo por cuenta (5 fallos) no frena probar una contraseña contra muchas cuentas.
    options.AddPolicy(AuthEndpoints.PoliticaEntrar, http => RateLimitPartition.GetFixedWindowLimiter(
        IpCliente(http), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("Auth:EntradasPorMinuto", 10),
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));
    // Renovar no se puede adivinar (token de 256 bits), pero cada intento consulta la base: se limita igual.
    options.AddPolicy(AuthEndpoints.PoliticaRenovar, http => RateLimitPartition.GetFixedWindowLimiter(
        IpCliente(http), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("Auth:RenovacionesPorMinuto", 30),
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));
    // Límite global de demos por minuto: cada una crea un usuario y datos en la base gratuita.
    options.AddFixedWindowLimiter(DemoEndpoints.Politica, limite =>
    {
        limite.PermitLimit = builder.Configuration.GetValue("Demo:PorMinuto", 10);
        limite.Window = TimeSpan.FromMinutes(1);
        limite.QueueLimit = 0;
    });
    // Límite global para los enlaces públicos: frena a quien intente adivinar tokens.
    options.AddFixedWindowLimiter(PublicoEndpoints.Politica, limite =>
    {
        limite.PermitLimit = builder.Configuration.GetValue("Publico:PeticionesPorMinuto", 120);
        limite.Window = TimeSpan.FromMinutes(1);
        limite.QueueLimit = 0;
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<HitoDbContext>("database", tags: ["ready"]);

// Sin este límite, EnableRetryOnFailure hace que /health/ready tarde ~1 min en responder 503.
builder.Services.Configure<HealthCheckServiceOptions>(options =>
    options.Registrations.Single(r => r.Name == "database").Timeout = TimeSpan.FromSeconds(5));

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// live: el proceso responde. No toca la base, así un fallo de la base no provoca reinicios del contenedor.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// ready: el proceso puede atender peticiones reales, incluida la base de datos.
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.MapAuth();
app.MapClientes();
app.MapProyectos();
app.MapPublico();
app.MapCobros();
app.MapDemo();

app.Run();

// IP del cliente para los límites por IP. Vercel y Render la ponen al inicio de X-Forwarded-For.
// Quien llame directo a Render puede falsearla: por eso el bloqueo por cuenta sigue siendo la defensa de fondo.
static string IpCliente(HttpContext http)
{
    var primera = http.Request.Headers["X-Forwarded-For"].ToString().Split(',', 2)[0].Trim();
    return primera.Length > 0 ? primera : http.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
}
