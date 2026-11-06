namespace Hito.Domain.Proyectos;

// Lo que el freelancer propone para un hito. HitoId es null para un hito nuevo.
public sealed record DatosDeHito(Guid? HitoId, string Titulo, decimal Monto, DateOnly FechaEntrega, string Entregable)
{
    public const int MaximoTitulo = 120;
    public const int MaximoEntregable = 2000;
    public const decimal MontoMaximo = 10_000_000m;
    public const int MaximoHitos = 30;

    // Errores por campo. La API los devuelve como 400; el dominio los usa para no aceptar datos inválidos.
    public IEnumerable<(string Campo, string Mensaje)> Errores()
    {
        if (string.IsNullOrWhiteSpace(Titulo) || Titulo.Length > MaximoTitulo)
            yield return ("titulo", $"Escribe un título (máximo {MaximoTitulo} caracteres).");
        if (Monto <= 0 || Monto > MontoMaximo)
            yield return ("monto", "El monto debe ser mayor que cero y menor que $10,000,000.00.");
        else if (decimal.Round(Monto, 2) != Monto)
            yield return ("monto", "El monto admite como máximo dos decimales.");
        if (FechaEntrega == default)
            yield return ("fechaEntrega", "Elige la fecha de entrega.");
        if (string.IsNullOrWhiteSpace(Entregable) || Entregable.Length > MaximoEntregable)
            yield return ("entregable", $"Describe qué se entrega (máximo {MaximoEntregable} caracteres).");
    }
}
