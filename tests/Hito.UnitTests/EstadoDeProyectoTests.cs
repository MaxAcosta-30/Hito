using Hito.Domain.Comun;
using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public sealed class EstadoDeProyectoTests
{
    [Fact]
    public void Cancelar_un_borrador_lo_deja_cancelado()
    {
        var proyecto = Datos.Proyecto();

        proyecto.Cancelar(Datos.Ahora);

        Assert.Equal(EstadoProyecto.Cancelado, proyecto.Estado);
    }

    [Fact]
    public void Un_proyecto_cancelado_no_se_edita_ni_se_vuelve_a_cancelar()
    {
        var proyecto = Datos.Proyecto();
        proyecto.Cancelar(Datos.Ahora);

        Assert.Throws<ReglaDeNegocioException>(() => proyecto.DefinirAlcance("X", [Datos.Hito()], Datos.Ahora));
        Assert.Throws<ReglaDeNegocioException>(() => proyecto.Cancelar(Datos.Ahora));
    }
}
