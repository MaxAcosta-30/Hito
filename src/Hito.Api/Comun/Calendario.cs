namespace Hito.Api.Comun;

// "Hoy" para el negocio es la fecha en México, no en UTC: a las 7 p. m. en Culiacán ya es mañana en UTC
// y un hito se vería vencido un día antes. La imagen chiseled-extra trae las zonas horarias.
public static class Calendario
{
    private static readonly TimeZoneInfo Mexico = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");

    public static DateOnly Hoy(TimeProvider reloj) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(reloj.GetUtcNow(), Mexico).DateTime);
}
