using Hito.Domain.Comun;
using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public sealed class AceptacionTests
{
    private static Proyecto Enviado()
    {
        var proyecto = Datos.Proyecto(Datos.Hito("Diseño", 8000m), Datos.Hito("Landing", 12000m));
        proyecto.Enviar(Datos.Ahora);
        return proyecto;
    }

    [Fact]
    public void Aceptar_congela_los_hitos_y_montos_de_la_version()
    {
        var proyecto = Enviado();

        proyecto.Aceptar(1, Datos.Ahora);

        Assert.Equal(EstadoProyecto.Aceptado, proyecto.Estado);
        Assert.Equal(1, proyecto.VersionVigente!.Numero);
        Assert.Equal(Datos.Ahora, proyecto.VersionVigente.AceptadaEn);
        // Ya no hay borrador que editar: cualquier cambio exige una versión nueva.
        Assert.Throws<ReglaDeNegocioException>(() => proyecto.DefinirAlcance("X", [Datos.Hito()], Datos.Ahora));
    }

    [Fact]
    public void Un_cambio_despues_de_aceptar_crea_una_version_nueva_sin_tocar_la_aceptada()
    {
        var proyecto = Enviado();
        proyecto.Aceptar(1, Datos.Ahora);
        var diseno = proyecto.VersionVigente!.Terminos.Single(t => t.Titulo == "Diseño").HitoId;

        var v2 = proyecto.NuevaVersion(Datos.Ahora);
        proyecto.DefinirAlcance("Tienda", [Datos.Hito("Diseño", 9000m, diseno)], Datos.Ahora);

        Assert.Equal(2, v2.Numero);
        Assert.Equal(EstadoVersion.Borrador, v2.Estado);
        Assert.Equal(9000m, v2.Total);
        // La versión 1 sigue intacta y vigente.
        Assert.Equal(20000m, proyecto.VersionVigente.Total);
        Assert.Equal(1, proyecto.VersionActual.Numero);
        Assert.Equal(20000m, proyecto.Total);
        // "Landing" sigue existiendo: la versión 1 lo menciona.
        Assert.Equal(2, proyecto.Hitos.Count);
    }

    [Fact]
    public void La_version_nueva_rige_solo_cuando_el_cliente_la_acepta()
    {
        var proyecto = Enviado();
        proyecto.Aceptar(1, Datos.Ahora);
        proyecto.NuevaVersion(Datos.Ahora);
        proyecto.DefinirAlcance("Tienda", [Datos.Hito("Todo junto", 15000m)], Datos.Ahora);
        proyecto.Enviar(Datos.Ahora);

        Assert.Equal(1, proyecto.VersionVigente!.Numero);

        proyecto.Aceptar(2, Datos.Ahora);

        Assert.Equal(2, proyecto.VersionVigente!.Numero);
        Assert.Equal(15000m, proyecto.Total);
        Assert.Equal(EstadoVersion.Aceptada, proyecto.Versiones.Single(v => v.Numero == 1).Estado);
        Assert.Equal(EstadoProyecto.Aceptado, proyecto.Estado);
    }

    [Fact]
    public void Solo_se_acepta_la_ultima_version_enviada()
    {
        var proyecto = Enviado();

        Assert.Throws<ReglaDeNegocioException>(() => proyecto.Aceptar(2, Datos.Ahora));
        proyecto.Aceptar(1, Datos.Ahora);
        Assert.Throws<ReglaDeNegocioException>(() => proyecto.Aceptar(1, Datos.Ahora));
    }

    [Fact]
    public void Cambiar_una_version_enviada_sin_aceptar_la_reemplaza_y_ya_no_se_puede_aceptar()
    {
        var proyecto = Enviado();

        proyecto.NuevaVersion(Datos.Ahora);

        Assert.Equal(EstadoVersion.Reemplazada, proyecto.Versiones.Single(v => v.Numero == 1).Estado);
        Assert.Throws<ReglaDeNegocioException>(() => proyecto.Aceptar(1, Datos.Ahora));
        Assert.Equal(EstadoProyecto.Enviado, proyecto.Estado);
    }

    [Fact]
    public void No_se_crea_otra_version_si_ya_hay_un_borrador()
    {
        var proyecto = Datos.Proyecto();

        Assert.Throws<ReglaDeNegocioException>(() => proyecto.NuevaVersion(Datos.Ahora));
    }

    [Fact]
    public void Un_borrador_no_se_acepta_ni_un_cancelado_se_acepta()
    {
        var borrador = Datos.Proyecto();
        Assert.Throws<ReglaDeNegocioException>(() => borrador.Aceptar(1, Datos.Ahora));

        var cancelado = Enviado();
        cancelado.Cancelar(Datos.Ahora);
        Assert.Throws<ReglaDeNegocioException>(() => cancelado.Aceptar(1, Datos.Ahora));
    }
}
