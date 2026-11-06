namespace Hito.Infrastructure.Identidad;

// Un refresh token emitido. Solo se guarda el hash: si se filtra la base, los tokens no sirven.
public sealed class SesionDeRenovacion
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UsuarioId { get; init; }
    public required string HashDelToken { get; init; }
    public required DateTimeOffset CreadaEn { get; init; }
    public required DateTimeOffset ExpiraEn { get; init; }
    public DateTimeOffset? RevocadaEn { get; set; }
}
