using System.Security.Cryptography;
using System.Text;
using Hito.Infrastructure.Identidad;
using Hito.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hito.Api.Auth;

// Refresh tokens: aleatorios, guardados como hash, de un solo uso (rotación) y con detección de reutilización.
public sealed class Sesiones(HitoDbContext db, TimeProvider reloj)
{
    public static readonly TimeSpan Duracion = TimeSpan.FromDays(7);

    public async Task<(string Token, DateTimeOffset ExpiraEn)> CrearAsync(Guid usuarioId, CancellationToken ct)
    {
        var token = Base64Url(RandomNumberGenerator.GetBytes(32));
        var ahora = reloj.GetUtcNow();
        var sesion = new SesionDeRenovacion
        {
            UsuarioId = usuarioId,
            HashDelToken = Hash(token),
            CreadaEn = ahora,
            ExpiraEn = ahora + Duracion,
        };
        db.Sesiones.Add(sesion);
        await db.SaveChangesAsync(ct);
        return (token, sesion.ExpiraEn);
    }

    // Devuelve el usuario y un token nuevo, o null si el token no sirve.
    public async Task<(Guid UsuarioId, string Token, DateTimeOffset ExpiraEn)?> RotarAsync(string token, CancellationToken ct)
    {
        var sesion = await db.Sesiones.SingleOrDefaultAsync(s => s.HashDelToken == Hash(token), ct);
        if (sesion is null) return null;

        var ahora = reloj.GetUtcNow();
        if (sesion.RevocadaEn is not null)
        {
            // Alguien usó un token ya rotado: probablemente lo robaron. Se cierran todas las sesiones del usuario.
            await db.Sesiones
                .Where(s => s.UsuarioId == sesion.UsuarioId && s.RevocadaEn == null)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevocadaEn, ahora), ct);
            return null;
        }
        if (sesion.ExpiraEn <= ahora) return null;

        sesion.RevocadaEn = ahora;
        var (nuevo, expira) = await CrearAsync(sesion.UsuarioId, ct);
        return (sesion.UsuarioId, nuevo, expira);
    }

    public Task RevocarAsync(string token, CancellationToken ct) =>
        db.Sesiones
            .Where(s => s.HashDelToken == Hash(token) && s.RevocadaEn == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevocadaEn, reloj.GetUtcNow()), ct);

    private static string Hash(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
