using Hito.Domain.Comun;
using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public sealed class HistorialTests
{
    [Fact]
    public void Cada_accion_deja_un_evento_con_quien_que_y_cuando()
    {
        var p = Datos.Proyecto(Datos.Hito("Diseño", 8000m));
        var eventos = new List<EventoDeProyecto>(p.EventosNuevos);
        void Tomar() { eventos.AddRange(p.EventosNuevos); p.OlvidarEventosNuevos(); }
        p.OlvidarEventosNuevos();
        var diseno = p.Hitos[0].Id;

        p.Enviar(Datos.Ahora); Tomar();
        p.Aceptar(1, Datos.Ahora.AddMinutes(1)); Tomar();
        p.EntregarHito(diseno, "https://marca.mx", Datos.Ahora); Tomar();
        p.PedirCambios(diseno, "Más contraste", Datos.Ahora); Tomar();
        p.EntregarHito(diseno, "https://marca.mx/v2", Datos.Ahora); Tomar();
        p.AprobarHito(diseno, Datos.Ahora); Tomar();
        p.RegistrarPago(diseno, 8000m, new DateOnly(2026, 11, 6), "Transferencia", Datos.Ahora); Tomar();

        Assert.Equal(
            [TipoDeEvento.Creado, TipoDeEvento.VersionEnviada, TipoDeEvento.VersionAceptada, TipoDeEvento.HitoEntregado,
             TipoDeEvento.CambiosSolicitados, TipoDeEvento.HitoEntregado, TipoDeEvento.HitoAprobado,
             TipoDeEvento.PagoRegistrado, TipoDeEvento.ProyectoCompletado],
            eventos.Select(e => e.Tipo));
        var aceptado = eventos.Single(e => e.Tipo == TipoDeEvento.VersionAceptada);
        Assert.Equal(Actor.Cliente, aceptado.Actor);
        Assert.Equal(Datos.Ahora.AddMinutes(1), aceptado.Fecha);
        Assert.Equal("aceptó la versión 1 del alcance por $8,000.00.", aceptado.Descripcion);
        Assert.Contains("«Más contraste»", eventos.Single(e => e.Tipo == TipoDeEvento.CambiosSolicitados).Descripcion);
        Assert.Equal("registró un pago de $8,000.00 en «Diseño» (Transferencia).", eventos.Single(e => e.Tipo == TipoDeEvento.PagoRegistrado).Descripcion);
        Assert.All(eventos, e => Assert.Equal(p.Id, e.ProyectoId));
    }

    [Fact]
    public void Crear_deja_un_solo_evento()
    {
        var p = Datos.Proyecto(Datos.Hito("A", 1m), Datos.Hito("B", 2m));

        var evento = Assert.Single(p.EventosNuevos);
        Assert.Equal("creó el proyecto con 2 hitos por $3.00.", evento.Descripcion);
        Assert.Equal(Actor.Freelancer, evento.Actor);
    }

    [Fact]
    public void Una_accion_rechazada_no_deja_evento()
    {
        var p = Datos.Proyecto();
        p.OlvidarEventosNuevos();

        Assert.Throws<ReglaDeNegocioException>(() => p.Aceptar(1, Datos.Ahora));

        Assert.Empty(p.EventosNuevos);
    }
}
