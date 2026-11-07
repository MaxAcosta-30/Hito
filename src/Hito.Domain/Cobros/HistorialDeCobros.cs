using Hito.Domain.Proyectos;

namespace Hito.Domain.Cobros;

// Un mes de la gráfica: lo acordado que ya vencía y lo cobrado, ambos acumulados al cierre del mes.
public sealed record MesDeCobro(DateOnly Mes, decimal Acordado, decimal Cobrado);

public static class HistorialDeCobros
{
    // Cuentan los proyectos aceptados y completados, con los términos de su versión vigente.
    // Los cancelados no: su alcance dejó de ser un compromiso. El mes en curso se corta en "hoy".
    public static IReadOnlyList<MesDeCobro> Calcular(IEnumerable<Proyecto> proyectos, DateOnly hoy, int meses = 6)
    {
        var vigentes = proyectos
            .Where(p => p.Estado is EstadoProyecto.Aceptado or EstadoProyecto.Completado && p.VersionVigente is not null)
            .ToList();
        var terminos = vigentes.SelectMany(p => p.VersionVigente!.Terminos).ToList();
        var pagos = vigentes.SelectMany(p => p.Pagos).ToList();

        var primerDia = new DateOnly(hoy.Year, hoy.Month, 1);
        return Enumerable.Range(0, meses)
            .Select(i => primerDia.AddMonths(i - meses + 1))
            .Select(mes =>
            {
                var finDeMes = mes.AddMonths(1).AddDays(-1);
                var corte = finDeMes < hoy ? finDeMes : hoy;
                return new MesDeCobro(
                    mes,
                    terminos.Where(t => t.FechaEntrega <= corte).Sum(t => t.Monto),
                    pagos.Where(p => p.Fecha <= corte).Sum(p => p.Monto));
            })
            .ToList();
    }
}
