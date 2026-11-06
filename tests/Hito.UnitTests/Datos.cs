using Hito.Domain.Proyectos;

namespace Hito.UnitTests;

public static class Datos
{
    public static readonly DateTimeOffset Ahora = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);
    public static readonly Guid Dueno = Guid.NewGuid();

    public static DatosDeHito Hito(string titulo = "Diseño", decimal monto = 1000m, Guid? id = null) =>
        new(id, titulo, monto, new DateOnly(2026, 12, 1), "Archivos fuente y guía de marca");

    public static Proyecto Proyecto(params DatosDeHito[] hitos) =>
        Domain.Proyectos.Proyecto.Crear(Dueno, Guid.NewGuid(), "Tienda en línea",
            hitos.Length > 0 ? hitos : [Hito()], Ahora);
}
