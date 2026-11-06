using Hito.Domain.Comun;
using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public sealed class HitoTests
{
    private const string Url = "https://tienda.rosales.mx";

    // Proyecto aceptado con dos hitos: Diseño ($8,000) y Landing ($12,000).
    internal static (Proyecto Proyecto, Guid Diseno, Guid Landing) Aceptado()
    {
        var proyecto = Datos.Proyecto(Datos.Hito("Diseño", 8000m), Datos.Hito("Landing", 12000m));
        proyecto.Enviar(Datos.Ahora);
        proyecto.Aceptar(1, Datos.Ahora);
        var terminos = proyecto.VersionVigente!.Terminos;
        return (proyecto, terminos.Single(t => t.Titulo == "Diseño").HitoId, terminos.Single(t => t.Titulo == "Landing").HitoId);
    }

    private static EstadoHito EstadoDe(Proyecto p, Guid id) => p.Hitos.Single(h => h.Id == id).Estado;

    [Fact]
    public void Flujo_feliz_pendiente_entregado_aprobado()
    {
        var (p, diseno, _) = Aceptado();

        p.EntregarHito(diseno, Url, Datos.Ahora);
        Assert.Equal(EstadoHito.Entregado, EstadoDe(p, diseno));
        Assert.Equal(Url, p.Hitos.Single(h => h.Id == diseno).EvidenciaUrl);

        p.AprobarHito(diseno, Datos.Ahora);
        Assert.Equal(EstadoHito.Aprobado, EstadoDe(p, diseno));
    }

    [Fact]
    public void Pedir_cambios_exige_comentario_y_permite_volver_a_entregar()
    {
        var (p, diseno, _) = Aceptado();
        p.EntregarHito(diseno, Url, Datos.Ahora);

        Assert.Throws<ReglaDeNegocioException>(() => p.PedirCambios(diseno, "   ", Datos.Ahora));
        p.PedirCambios(diseno, "El logo debe ir en azul", Datos.Ahora);
        Assert.Equal(EstadoHito.CambiosSolicitados, EstadoDe(p, diseno));
        Assert.Equal("El logo debe ir en azul", p.Hitos.Single(h => h.Id == diseno).ComentarioCambios);

        p.EntregarHito(diseno, Url + "/v2", Datos.Ahora);
        Assert.Equal(EstadoHito.Entregado, EstadoDe(p, diseno));
    }

    [Fact]
    public void Transiciones_invalidas_se_rechazan()
    {
        var (p, diseno, _) = Aceptado();

        Assert.Throws<ReglaDeNegocioException>(() => p.AprobarHito(diseno, Datos.Ahora));        // Pendiente → Aprobado
        Assert.Throws<ReglaDeNegocioException>(() => p.PedirCambios(diseno, "x", Datos.Ahora));  // Pendiente → Cambios
        p.EntregarHito(diseno, Url, Datos.Ahora);
        Assert.Throws<ReglaDeNegocioException>(() => p.EntregarHito(diseno, Url, Datos.Ahora));  // Entregado → Entregado
        p.AprobarHito(diseno, Datos.Ahora);
        Assert.Throws<ReglaDeNegocioException>(() => p.EntregarHito(diseno, Url, Datos.Ahora));  // Aprobado → Entregado
        Assert.Throws<ReglaDeNegocioException>(() => p.PedirCambios(diseno, "x", Datos.Ahora));  // Aprobado → Cambios
        Assert.Throws<ReglaDeNegocioException>(() => p.AprobarHito(diseno, Datos.Ahora));        // Aprobado → Aprobado
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,hola")]
    [InlineData("ftp://archivos.mx/a.zip")]
    [InlineData("tienda.rosales.mx")]
    [InlineData("")]
    public void La_evidencia_solo_acepta_http_o_https(string url)
    {
        var (p, diseno, _) = Aceptado();

        Assert.False(HitoDeProyecto.UrlValida(url));
        Assert.Throws<ReglaDeNegocioException>(() => p.EntregarHito(diseno, url, Datos.Ahora));
    }

    [Fact]
    public void Sin_alcance_aceptado_no_se_entrega_nada()
    {
        var proyecto = Datos.Proyecto();
        proyecto.Enviar(Datos.Ahora);

        Assert.Throws<ReglaDeNegocioException>(() => proyecto.EntregarHito(proyecto.Hitos[0].Id, Url, Datos.Ahora));
    }

    [Fact]
    public void Un_hito_de_una_version_sin_aceptar_no_se_entrega()
    {
        var (p, _, _) = Aceptado();
        p.NuevaVersion(Datos.Ahora);
        var ids = p.UltimaVersion.Terminos.Select(t => (Guid?)t.HitoId).ToList();
        p.DefinirAlcance("Tienda", [.. ids.Select(id => Datos.Hito(id: id)), Datos.Hito("Extra")], Datos.Ahora);
        var extra = p.UltimaVersion.Terminos.Single(t => t.Titulo == "Extra").HitoId;

        Assert.Throws<ReglaDeNegocioException>(() => p.EntregarHito(extra, Url, Datos.Ahora));
    }

    // Reglas del caso 5 que dependen de hitos avanzados.
    [Fact]
    public void Una_version_nueva_no_puede_cambiar_ni_quitar_un_hito_que_ya_avanzo()
    {
        var (p, diseno, landing) = Aceptado();
        p.EntregarHito(diseno, Url, Datos.Ahora);
        p.NuevaVersion(Datos.Ahora);

        Assert.Throws<ReglaDeNegocioException>(() => p.DefinirAlcance("Tienda",
            [Datos.Hito("Diseño", 1m, diseno), Datos.Hito("Landing", 12000m, landing)], Datos.Ahora));
        Assert.Throws<ReglaDeNegocioException>(() => p.DefinirAlcance("Tienda",
            [Datos.Hito("Landing", 12000m, landing)], Datos.Ahora));
    }

    [Fact]
    public void Aceptar_se_rechaza_si_un_hito_avanzo_mientras_la_version_esperaba()
    {
        var (p, diseno, landing) = Aceptado();
        p.NuevaVersion(Datos.Ahora);
        p.DefinirAlcance("Tienda", [Datos.Hito("Diseño", 9000m, diseno), Datos.Hito("Landing", 12000m, landing)], Datos.Ahora);
        p.Enviar(Datos.Ahora);
        // Mientras el cliente revisa la versión 2, el hito se entrega bajo la versión 1.
        p.EntregarHito(diseno, Url, Datos.Ahora);

        Assert.Throws<ReglaDeNegocioException>(() => p.Aceptar(2, Datos.Ahora));
    }
}
