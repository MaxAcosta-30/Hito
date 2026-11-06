using Hito.Domain.Comun;
using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public sealed class AlcanceTests
{
    [Fact]
    public void Crear_un_proyecto_deja_la_version_1_en_borrador_con_el_total_calculado()
    {
        var proyecto = Datos.Proyecto(Datos.Hito("Diseño", 8000m), Datos.Hito("Landing", 12000.50m));

        Assert.Equal(EstadoProyecto.Borrador, proyecto.Estado);
        var version = Assert.Single(proyecto.Versiones);
        Assert.Equal(1, version.Numero);
        Assert.Equal(EstadoVersion.Borrador, version.Estado);
        Assert.Equal(20000.50m, proyecto.Total);
        Assert.Equal(2, proyecto.Hitos.Count);
        Assert.All(proyecto.Hitos, h => Assert.Equal(EstadoHito.Pendiente, h.Estado));
    }

    [Fact]
    public void El_total_suma_montos_decimales_sin_errores_de_redondeo()
    {
        // Con double, 0.1 + 0.2 no da 0.3; con decimal sí.
        var proyecto = Datos.Proyecto(Datos.Hito("A", 0.10m), Datos.Hito("B", 0.20m));

        Assert.Equal(0.30m, proyecto.Total);
    }

    [Fact]
    public void Definir_alcance_actualiza_agrega_y_quita_hitos_del_borrador()
    {
        var proyecto = Datos.Proyecto(Datos.Hito("Diseño", 1000m), Datos.Hito("Landing", 2000m));
        var diseno = proyecto.UltimaVersion.Terminos.Single(t => t.Titulo == "Diseño").HitoId;

        proyecto.DefinirAlcance("Tienda v2",
            [Datos.Hito("Capacitación", 500m), Datos.Hito("Diseño final", 1500m, diseno)], Datos.Ahora);

        var terminos = proyecto.UltimaVersion.Terminos.OrderBy(t => t.Orden).ToList();
        Assert.Equal(["Capacitación", "Diseño final"], terminos.Select(t => t.Titulo));
        Assert.Equal(diseno, terminos[1].HitoId);
        Assert.Equal(2000m, proyecto.Total);
        Assert.Equal("Tienda v2", proyecto.Nombre);
        // "Landing" solo existía en este borrador: el hito desaparece.
        Assert.Equal(2, proyecto.Hitos.Count);
    }

    [Fact]
    public void Un_proyecto_necesita_al_menos_un_hito()
    {
        var proyecto = Datos.Proyecto();

        Assert.Throws<ReglaDeNegocioException>(() => proyecto.DefinirAlcance("Tienda", [], Datos.Ahora));
    }

    [Fact]
    public void Un_hito_repetido_o_de_otro_proyecto_se_rechaza()
    {
        var proyecto = Datos.Proyecto();
        var id = proyecto.Hitos[0].Id;

        Assert.Throws<ReglaDeNegocioException>(() =>
            proyecto.DefinirAlcance("Tienda", [Datos.Hito(id: id), Datos.Hito(id: id)], Datos.Ahora));
        Assert.Throws<ReglaDeNegocioException>(() =>
            proyecto.DefinirAlcance("Tienda", [Datos.Hito(id: Guid.NewGuid())], Datos.Ahora));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10.001)]
    [InlineData(10_000_000.01)]
    public void Montos_invalidos_se_reportan_en_su_campo(double monto)
    {
        var datos = Datos.Hito(monto: (decimal)monto);

        Assert.Contains(datos.Errores(), e => e.Campo == "monto");
        Assert.Throws<ArgumentException>(() => Datos.Proyecto(datos));
    }

    [Fact]
    public void Un_hito_valido_no_tiene_errores()
    {
        Assert.Empty(Datos.Hito(monto: 10_000_000m).Errores());
    }
}
