using Hito.Domain.Comun;
using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public sealed class EnvioTests
{
    [Fact]
    public void Enviar_pasa_el_proyecto_a_enviado_y_congela_la_edicion_del_alcance()
    {
        var proyecto = Datos.Proyecto();

        proyecto.Enviar(Datos.Ahora);

        Assert.Equal(EstadoProyecto.Enviado, proyecto.Estado);
        Assert.Equal(EstadoVersion.Enviada, proyecto.UltimaVersion.Estado);
        Assert.Equal(Datos.Ahora, proyecto.UltimaVersion.EnviadaEn);
        Assert.Throws<ReglaDeNegocioException>(() => proyecto.DefinirAlcance("X", [Datos.Hito()], Datos.Ahora));
    }

    [Fact]
    public void No_se_puede_enviar_dos_veces_la_misma_version()
    {
        var proyecto = Datos.Proyecto();
        proyecto.Enviar(Datos.Ahora);

        Assert.Throws<ReglaDeNegocioException>(() => proyecto.Enviar(Datos.Ahora));
    }

    [Fact]
    public void Un_borrador_no_puede_compartirse_ni_un_cancelado_enviarse()
    {
        var borrador = Datos.Proyecto();
        Assert.Throws<ReglaDeNegocioException>(borrador.ExigirQuePuedaCompartirse);

        borrador.Cancelar(Datos.Ahora);
        Assert.Throws<ReglaDeNegocioException>(() => borrador.Enviar(Datos.Ahora));
    }

    [Fact]
    public void Un_enlace_vale_30_dias_y_deja_de_valer_al_revocarse()
    {
        var enlace = new EnlaceDeCliente(Datos.Dueno, Guid.NewGuid(), "hash", Datos.Ahora);

        Assert.True(enlace.EsValido(Datos.Ahora.AddDays(29)));
        Assert.False(enlace.EsValido(Datos.Ahora.AddDays(30)));

        enlace.Revocar(Datos.Ahora.AddDays(1));
        Assert.False(enlace.EsValido(Datos.Ahora.AddDays(2)));
    }
}
