using Hito.Domain.Clientes;
using Hito.Domain.Proyectos;
using Hito.Infrastructure.Persistence;

namespace Hito.Api.Demo;

// Datos de ejemplo creados con los mismos métodos del dominio que usa la app, con fechas escalonadas:
// así el panel, los sellos y el historial se ven reales y respetan todas las reglas.
public static class DatosDeDemo
{
    public static void Sembrar(HitoDbContext db, Guid dueno, DateOnly hoy, DateTimeOffset ahora)
    {
        DateTimeOffset Hace(int dias, int horas = 0) => ahora.AddDays(-dias).AddHours(-horas);

        var rosales = new Cliente(dueno, "Panadería Rosales", "Rosales SA de CV", "ana@rosales.mx", "+52 667 123 4567", Hace(41));
        var clinica = new Cliente(dueno, "Clínica Norte", null, "contacto@clinicanorte.mx", null, Hace(21));
        var vega = new Cliente(dueno, "Estudio Vega", "Vega Arquitectos", null, "+52 667 765 4321", Hace(3));
        db.Clientes.AddRange(rosales, clinica, vega);

        // 1. Aceptado y en marcha: un hito pagado, uno aprobado sin pagar (y vencido), uno entregado y uno pendiente.
        var tienda = Proyecto.Crear(dueno, rosales.Id, "Tienda en línea",
        [
            Hito("Diseño de marca", 8000m, hoy.AddDays(-30), "Logotipo, paleta de colores y guía de marca en PDF."),
            Hito("Landing page", 12000m, hoy.AddDays(-5), "Página de inicio publicada con formulario de contacto."),
            Hito("Tienda en línea", 20000m, hoy.AddDays(10), "Catálogo, carrito y pagos de prueba funcionando."),
            Hito("Capacitación", 4000m, hoy.AddDays(25), "Dos sesiones de una hora para administrar la tienda."),
        ], Hace(40));
        tienda.Enviar(Hace(39));
        tienda.Aceptar(1, Hace(38));
        var t = Ids(tienda);
        tienda.EntregarHito(t["Diseño de marca"], "https://www.figma.com/", Hace(28));
        tienda.AprobarHito(t["Diseño de marca"], Hace(27));
        tienda.RegistrarPago(t["Diseño de marca"], 8000m, hoy.AddDays(-25), "Transferencia", Hace(25));
        tienda.EntregarHito(t["Landing page"], "https://example.com/", Hace(6));
        tienda.AprobarHito(t["Landing page"], Hace(4));
        tienda.EntregarHito(t["Tienda en línea"], "https://example.org/", Hace(1, 3));

        // 2. Aceptado con cambios solicitados en un hito vencido.
        var citas = Proyecto.Crear(dueno, clinica.Id, "App de citas · Fase 1",
        [
            Hito("Prototipo navegable", 15000m, hoy.AddDays(-2), "Prototipo en Figma con el flujo de agendar una cita."),
            Hito("Agenda y recordatorios", 25000m, hoy.AddDays(20), "App web con agenda por doctor y recordatorios."),
        ], Hace(20));
        citas.Enviar(Hace(19));
        citas.Aceptar(1, Hace(18));
        var c = Ids(citas);
        citas.EntregarHito(c["Prototipo navegable"], "https://www.figma.com/", Hace(5));
        citas.PedirCambios(c["Prototipo navegable"], "Agreguen un recordatorio por WhatsApp un día antes de la cita.", Hace(3));

        // 3. Borrador sin enviar.
        var sitio = Proyecto.Crear(dueno, vega.Id, "Sitio institucional",
        [
            Hito("Arquitectura de información", 6000m, hoy.AddDays(14), "Mapa del sitio y contenidos por página."),
            Hito("Diseño visual", 9000m, hoy.AddDays(28), "Diseño de inicio, proyectos y contacto."),
            Hito("Desarrollo", 14000m, hoy.AddDays(45), "Sitio publicado y administrable."),
        ], Hace(2));

        db.Proyectos.AddRange(tienda, citas, sitio);
    }

    private static DatosDeHito Hito(string titulo, decimal monto, DateOnly fecha, string entregable) =>
        new(null, titulo, monto, fecha, entregable);

    private static Dictionary<string, Guid> Ids(Proyecto p) =>
        p.VersionVigente!.Terminos.ToDictionary(t => t.Titulo, t => t.HitoId);
}
