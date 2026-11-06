using Hito.Domain.Proyectos;

namespace Hito.Domain.Cobros;

public enum MotivoDeAtencion { Vencido, AprobadoSinPagar, CambiosSolicitados, EsperaRevision }

public sealed record HitoQueAtender(
    Guid ProyectoId, string Proyecto, Guid ClienteId, Guid HitoId, string Titulo, decimal Pendiente,
    DateOnly FechaEntrega, EstadoHito Estado, MotivoDeAtencion Motivo);

public sealed record ResumenDeCobros(
    decimal PorCobrar, decimal AprobadoSinPagar, decimal Vencido, int ProyectosActivos, IReadOnlyList<HitoQueAtender> Atencion)
{
    // Solo cuentan los proyectos aceptados, con los términos de su versión vigente.
    // Pendiente de un hito = su monto menos lo ya pagado.
    public static ResumenDeCobros Calcular(IEnumerable<Proyecto> proyectos, DateOnly hoy)
    {
        decimal porCobrar = 0, aprobado = 0, vencido = 0;
        var activos = 0;
        var atencion = new List<HitoQueAtender>();

        foreach (var p in proyectos.Where(p => p.Estado == EstadoProyecto.Aceptado && p.VersionVigente is not null))
        {
            activos++;
            foreach (var t in p.VersionVigente!.Terminos)
            {
                var hito = p.Hitos.First(h => h.Id == t.HitoId);
                var pendiente = t.Monto - p.PagadoDe(t.HitoId);
                if (pendiente <= 0) continue;

                porCobrar += pendiente;
                var estaVencido = t.FechaEntrega < hoy;
                if (estaVencido) vencido += pendiente;
                if (hito.Estado == EstadoHito.Aprobado) aprobado += pendiente;

                MotivoDeAtencion? motivo = estaVencido ? MotivoDeAtencion.Vencido
                    : hito.Estado switch
                    {
                        EstadoHito.Aprobado => MotivoDeAtencion.AprobadoSinPagar,
                        EstadoHito.CambiosSolicitados => MotivoDeAtencion.CambiosSolicitados,
                        EstadoHito.Entregado => MotivoDeAtencion.EsperaRevision,
                        _ => null,
                    };
                if (motivo is { } m)
                    atencion.Add(new HitoQueAtender(p.Id, p.Nombre, p.ClienteId, t.HitoId, t.Titulo, pendiente, t.FechaEntrega, hito.Estado, m));
            }
        }

        return new ResumenDeCobros(porCobrar, aprobado, vencido, activos,
            atencion.OrderBy(a => a.Motivo).ThenBy(a => a.FechaEntrega).ToList());
    }
}
