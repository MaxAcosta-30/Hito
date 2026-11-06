using Hito.Domain.Comun;

namespace Hito.Domain.Clientes;

// Cliente del freelancer. No tiene cuenta: entra a sus proyectos con un enlace privado.
// Se archiva en lugar de borrarse, porque sus proyectos y pagos deben conservar su historial.
public sealed class Cliente : IPerteneceAUsuario
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid OwnerId { get; private set; }
    public string Nombre { get; private set; }
    public string? Empresa { get; private set; }
    public string? Email { get; private set; }
    public string? Telefono { get; private set; }
    public DateTimeOffset CreadoEn { get; private set; }
    public DateTimeOffset? ArchivadoEn { get; private set; }

    public bool Archivado => ArchivadoEn is not null;

    // Para EF Core al leer de la base.
    private Cliente() => Nombre = null!;

    public Cliente(Guid ownerId, string nombre, string? empresa, string? email, string? telefono, DateTimeOffset ahora)
    {
        OwnerId = ownerId;
        Nombre = nombre;
        Empresa = empresa;
        Email = email;
        Telefono = telefono;
        CreadoEn = ahora;
    }

    public void Actualizar(string nombre, string? empresa, string? email, string? telefono)
    {
        Nombre = nombre;
        Empresa = empresa;
        Email = email;
        Telefono = telefono;
    }

    // Archivar dos veces conserva la primera fecha.
    public void Archivar(DateTimeOffset ahora) => ArchivadoEn ??= ahora;

    public void Restaurar() => ArchivadoEn = null;
}
