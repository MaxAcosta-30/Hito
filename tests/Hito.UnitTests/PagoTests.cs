using Hito.Domain.Comun;
using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public sealed class PagoTests
{
    private static readonly DateOnly Hoy = new(2026, 11, 6);

    // Proyecto aceptado con "Diseño" ($8,000) aprobado y "Landing" ($12,000) pendiente.
    private static (Proyecto Proyecto, Guid Diseno, Guid Landing) ConDisenoAprobado()
    {
        var (p, diseno, landing) = HitoTests.Aceptado();
        p.EntregarHito(diseno, "https://marca.mx", Datos.Ahora);
        p.AprobarHito(diseno, Datos.Ahora);
        return (p, diseno, landing);
    }

    private static EstadoHito EstadoDe(Proyecto p, Guid id) => p.Hitos.Single(h => h.Id == id).Estado;

    [Fact]
    public void Solo_se_paga_un_hito_aprobado()
    {
        var (p, _, landing) = ConDisenoAprobado();

        Assert.Throws<ReglaDeNegocioException>(() => p.RegistrarPago(landing, 100m, Hoy, null, Datos.Ahora));
        p.EntregarHito(landing, "https://landing.mx", Datos.Ahora);
        Assert.Throws<ReglaDeNegocioException>(() => p.RegistrarPago(landing, 100m, Hoy, null, Datos.Ahora));
    }

    [Fact]
    public void No_se_puede_pagar_de_mas()
    {
        var (p, diseno, _) = ConDisenoAprobado();

        var error = Assert.Throws<ReglaDeNegocioException>(() => p.RegistrarPago(diseno, 8000.01m, Hoy, null, Datos.Ahora));
        Assert.Contains("8,000.00", error.Message);

        p.RegistrarPago(diseno, 5000m, Hoy, null, Datos.Ahora);
        Assert.Throws<ReglaDeNegocioException>(() => p.RegistrarPago(diseno, 3000.01m, Hoy, null, Datos.Ahora));
        Assert.Equal(5000m, p.PagadoDe(diseno));
    }

    [Fact]
    public void Un_pago_parcial_deja_el_hito_aprobado_y_el_resto_lo_marca_pagado()
    {
        var (p, diseno, _) = ConDisenoAprobado();

        p.RegistrarPago(diseno, 5000m, Hoy, "Anticipo", Datos.Ahora);
        Assert.Equal(EstadoHito.Aprobado, EstadoDe(p, diseno));

        p.RegistrarPago(diseno, 3000m, Hoy, null, Datos.Ahora);
        Assert.Equal(EstadoHito.Pagado, EstadoDe(p, diseno));
        Assert.Equal(8000m, p.Pagado);
        // Pagado es final: ni un centavo más.
        Assert.Throws<ReglaDeNegocioException>(() => p.RegistrarPago(diseno, 0.01m, Hoy, null, Datos.Ahora));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(10.005)]
    public void Montos_invalidos_se_rechazan(double monto)
    {
        var (p, diseno, _) = ConDisenoAprobado();

        Assert.Throws<ReglaDeNegocioException>(() => p.RegistrarPago(diseno, (decimal)monto, Hoy, null, Datos.Ahora));
    }

    [Fact]
    public void Pagar_todos_los_hitos_completa_el_proyecto()
    {
        var (p, diseno, landing) = ConDisenoAprobado();
        p.RegistrarPago(diseno, 8000m, Hoy, null, Datos.Ahora);
        Assert.Equal(EstadoProyecto.Aceptado, p.Estado);

        p.EntregarHito(landing, "https://landing.mx", Datos.Ahora);
        p.AprobarHito(landing, Datos.Ahora);
        p.RegistrarPago(landing, 12000m, Hoy, null, Datos.Ahora);

        Assert.Equal(EstadoProyecto.Completado, p.Estado);
        Assert.Throws<ReglaDeNegocioException>(() => p.NuevaVersion(Datos.Ahora));
    }

    [Fact]
    public void Un_proyecto_con_pagos_se_puede_cancelar_y_conserva_sus_pagos()
    {
        var (p, diseno, _) = ConDisenoAprobado();
        p.RegistrarPago(diseno, 2000m, Hoy, null, Datos.Ahora);

        p.Cancelar(Datos.Ahora);

        Assert.Equal(EstadoProyecto.Cancelado, p.Estado);
        Assert.Single(p.Pagos);
    }
}
