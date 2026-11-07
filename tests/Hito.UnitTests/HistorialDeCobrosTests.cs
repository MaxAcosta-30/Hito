using Hito.Domain.Cobros;
using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public sealed class HistorialDeCobrosTests
{
    private static readonly DateOnly Hoy = new(2026, 11, 6);

    private static (Proyecto P, Dictionary<string, Guid> Ids) Aceptado(params DatosDeHito[] hitos)
    {
        var p = Proyecto.Crear(Datos.Dueno, Guid.NewGuid(), "Proyecto", hitos, Datos.Ahora);
        p.Enviar(Datos.Ahora);
        p.Aceptar(1, Datos.Ahora);
        return (p, p.VersionVigente!.Terminos.ToDictionary(t => t.Titulo, t => t.HitoId));
    }

    private static void Pagar(Proyecto p, Guid hito, decimal monto, DateOnly fecha)
    {
        p.EntregarHito(hito, "https://x.mx", Datos.Ahora);
        p.AprobarHito(hito, Datos.Ahora);
        p.RegistrarPago(hito, monto, fecha, null, Datos.Ahora);
    }

    [Fact]
    public void Acumula_lo_acordado_por_fecha_de_entrega_y_lo_cobrado_por_fecha_de_pago()
    {
        var (p, ids) = Aceptado(
            new DatosDeHito(null, "Septiembre", 8000m, new DateOnly(2026, 9, 15), "x"),
            new DatosDeHito(null, "Octubre", 12000m, new DateOnly(2026, 10, 20), "x"),
            new DatosDeHito(null, "Diciembre", 5000m, new DateOnly(2026, 12, 1), "x"));
        Pagar(p, ids["Septiembre"], 8000m, new DateOnly(2026, 10, 2));

        var meses = HistorialDeCobros.Calcular([p], Hoy);

        Assert.Equal(6, meses.Count);
        Assert.Equal(new DateOnly(2026, 6, 1), meses[0].Mes);
        Assert.Equal(new DateOnly(2026, 11, 1), meses[^1].Mes);
        var porMes = meses.ToDictionary(m => m.Mes.Month);
        Assert.Equal((0m, 0m), (porMes[8].Acordado, porMes[8].Cobrado));
        Assert.Equal((8000m, 0m), (porMes[9].Acordado, porMes[9].Cobrado));
        Assert.Equal((20000m, 8000m), (porMes[10].Acordado, porMes[10].Cobrado));
        // Noviembre se corta en "hoy": el hito de diciembre todavía no cuenta.
        Assert.Equal((20000m, 8000m), (porMes[11].Acordado, porMes[11].Cobrado));
    }

    [Fact]
    public void Ignora_borradores_y_cancelados()
    {
        var borrador = Proyecto.Crear(Datos.Dueno, Guid.NewGuid(), "Borrador",
            [new DatosDeHito(null, "A", 1000m, new DateOnly(2026, 10, 1), "x")], Datos.Ahora);
        var (cancelado, _) = Aceptado(new DatosDeHito(null, "B", 3000m, new DateOnly(2026, 10, 1), "x"));
        cancelado.Cancelar(Datos.Ahora);

        var meses = HistorialDeCobros.Calcular([borrador, cancelado], Hoy);

        Assert.All(meses, m => Assert.Equal((0m, 0m), (m.Acordado, m.Cobrado)));
    }
}
