using Hito.Domain.Cobros;
using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public sealed class ResumenDeCobrosTests
{
    private static readonly DateOnly Hoy = new(2026, 11, 6);

    private static DatosDeHito Hito(string titulo, decimal monto, DateOnly fecha) =>
        new(null, titulo, monto, fecha, "Entrega");

    private static (Proyecto P, Dictionary<string, Guid> Ids) Aceptado(params DatosDeHito[] hitos)
    {
        var p = Proyecto.Crear(Datos.Dueno, Guid.NewGuid(), "Proyecto", hitos, Datos.Ahora);
        p.Enviar(Datos.Ahora);
        p.Aceptar(1, Datos.Ahora);
        return (p, p.VersionVigente!.Terminos.ToDictionary(t => t.Titulo, t => t.HitoId));
    }

    private static void Aprobar(Proyecto p, Guid hito)
    {
        p.EntregarHito(hito, "https://x.mx", Datos.Ahora);
        p.AprobarHito(hito, Datos.Ahora);
    }

    [Fact]
    public void Los_totales_suman_lo_pendiente_por_categoria()
    {
        var (a, ia) = Aceptado(
            Hito("Vencido sin entregar", 4000m, Hoy.AddDays(-7)),
            Hito("Aprobado con anticipo", 12000m, Hoy.AddDays(10)),
            Hito("Pagado", 8000m, Hoy.AddDays(-20)),
            Hito("Futuro", 20000m, Hoy.AddDays(30)));
        Aprobar(a, ia["Aprobado con anticipo"]);
        a.RegistrarPago(ia["Aprobado con anticipo"], 2000m, Hoy, null, Datos.Ahora);
        Aprobar(a, ia["Pagado"]);
        a.RegistrarPago(ia["Pagado"], 8000m, Hoy, null, Datos.Ahora);

        var r = ResumenDeCobros.Calcular([a], Hoy);

        Assert.Equal(4000m + 10000m + 20000m, r.PorCobrar);
        Assert.Equal(10000m, r.AprobadoSinPagar);
        Assert.Equal(4000m, r.Vencido);
        Assert.Equal(1, r.ProyectosActivos);
        Assert.Equal(["Vencido sin entregar", "Aprobado con anticipo"], r.Atencion.Select(x => x.Titulo));
        Assert.Equal(MotivoDeAtencion.Vencido, r.Atencion[0].Motivo);
    }

    [Fact]
    public void Un_hito_aprobado_y_vencido_cuenta_en_ambos_pero_una_sola_vez_en_por_cobrar()
    {
        var (a, ia) = Aceptado(Hito("Atrasado", 5000m, Hoy.AddDays(-1)));
        Aprobar(a, ia["Atrasado"]);

        var r = ResumenDeCobros.Calcular([a], Hoy);

        Assert.Equal(5000m, r.PorCobrar);
        Assert.Equal(5000m, r.AprobadoSinPagar);
        Assert.Equal(5000m, r.Vencido);
        Assert.Single(r.Atencion);
    }

    [Fact]
    public void Vence_al_dia_siguiente_de_la_fecha_de_entrega()
    {
        var (a, _) = Aceptado(Hito("Hoy", 1000m, Hoy));

        Assert.Equal(0m, ResumenDeCobros.Calcular([a], Hoy).Vencido);
        Assert.Equal(1000m, ResumenDeCobros.Calcular([a], Hoy.AddDays(1)).Vencido);
    }

    [Fact]
    public void Borradores_enviados_cancelados_y_completados_no_cuentan()
    {
        var borrador = Proyecto.Crear(Datos.Dueno, Guid.NewGuid(), "B", [Hito("x", 100m, Hoy)], Datos.Ahora);
        var enviado = Proyecto.Crear(Datos.Dueno, Guid.NewGuid(), "E", [Hito("x", 100m, Hoy)], Datos.Ahora);
        enviado.Enviar(Datos.Ahora);
        var (cancelado, _) = Aceptado(Hito("x", 100m, Hoy));
        cancelado.Cancelar(Datos.Ahora);
        var (completado, ic) = Aceptado(Hito("x", 100m, Hoy));
        Aprobar(completado, ic["x"]);
        completado.RegistrarPago(ic["x"], 100m, Hoy, null, Datos.Ahora);

        var r = ResumenDeCobros.Calcular([borrador, enviado, cancelado, completado], Hoy);

        Assert.Equal(0m, r.PorCobrar);
        Assert.Equal(0, r.ProyectosActivos);
        Assert.Empty(r.Atencion);
    }

    [Fact]
    public void Una_version_nueva_sin_aceptar_no_cambia_los_totales()
    {
        var (a, ia) = Aceptado(Hito("Diseño", 1000m, Hoy.AddDays(5)));
        a.NuevaVersion(Datos.Ahora);
        a.DefinirAlcance("Proyecto", [new DatosDeHito(ia["Diseño"], "Diseño", 9999m, Hoy.AddDays(5), "Entrega")], Datos.Ahora);

        Assert.Equal(1000m, ResumenDeCobros.Calcular([a], Hoy).PorCobrar);
    }
}
