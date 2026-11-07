using Hito.Api.Comun;
using Hito.Api.Proyectos;
using Hito.Domain.Cobros;
using Hito.Domain.Proyectos;
using Hito.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hito.Api.Cobros;

public static class CobrosEndpoints
{
    public sealed record HitoQueAtenderRespuesta(
        Guid ProyectoId, string Proyecto, string Cliente, Guid HitoId, string Titulo, decimal Pendiente,
        DateOnly FechaEntrega, EstadoHito Estado, MotivoDeAtencion Motivo);
    public sealed record PanelRespuesta(
        decimal PorCobrar, decimal AprobadoSinPagar, decimal Vencido, int ProyectosActivos, DateOnly Hoy,
        List<HitoQueAtenderRespuesta> Atencion, IReadOnlyList<MesDeCobro> Historial);

    public static void MapCobros(this IEndpointRouteBuilder app) =>
        app.MapGet("/cobros", Panel).RequireAuthorization();

    // Los totales se calculan en el dominio (ResumenDeCobros) con las mismas reglas que el detalle del proyecto.
    private static async Task<IResult> Panel(HitoDbContext db, TimeProvider reloj, CancellationToken ct)
    {
        // Aceptados para los totales; también completados para la gráfica histórica.
        var proyectos = await db.Proyectos
            .Where(p => p.Estado == EstadoProyecto.Aceptado || p.Estado == EstadoProyecto.Completado)
            .ConAgregado().ToListAsync(ct);
        var hoy = Calendario.Hoy(reloj);
        var resumen = ResumenDeCobros.Calcular(proyectos, hoy);

        var ids = resumen.Atencion.Select(a => a.ClienteId).Distinct().ToList();
        var clientes = await db.Clientes.Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Nombre, ct);
        return Results.Ok(new PanelRespuesta(
            resumen.PorCobrar, resumen.AprobadoSinPagar, resumen.Vencido, resumen.ProyectosActivos, hoy,
            resumen.Atencion.Select(a => new HitoQueAtenderRespuesta(
                a.ProyectoId, a.Proyecto, clientes[a.ClienteId], a.HitoId, a.Titulo, a.Pendiente, a.FechaEntrega, a.Estado, a.Motivo))
                .ToList(),
            HistorialDeCobros.Calcular(proyectos, hoy)));
    }
}
