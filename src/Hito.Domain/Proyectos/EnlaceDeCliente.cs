using Hito.Domain.Comun;

namespace Hito.Domain.Proyectos;

// Enlace privado con el que el cliente ve el proyecto sin cuenta. Solo se guarda el hash del token.
public sealed class EnlaceDeCliente : IPerteneceAUsuario
{
    public static readonly TimeSpan Duracion = TimeSpan.FromDays(30);

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public Guid ProyectoId { get; private set; }
    public string HashDelToken { get; private set; } = null!;
    public DateTimeOffset CreadoEn { get; private set; }
    public DateTimeOffset ExpiraEn { get; private set; }
    public DateTimeOffset? RevocadoEn { get; private set; }

    private EnlaceDeCliente() { }

    public EnlaceDeCliente(Guid ownerId, Guid proyectoId, string hashDelToken, DateTimeOffset ahora)
    {
        OwnerId = ownerId;
        ProyectoId = proyectoId;
        HashDelToken = hashDelToken;
        CreadoEn = ahora;
        ExpiraEn = ahora + Duracion;
    }

    public bool EsValido(DateTimeOffset ahora) => RevocadoEn is null && ahora < ExpiraEn;

    public void Revocar(DateTimeOffset ahora) => RevocadoEn ??= ahora;
}
