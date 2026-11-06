namespace Hito.Domain.Comun;

// Toda entidad del freelancer la implementa; HitoDbContext filtra por OwnerId en cada consulta.
public interface IPerteneceAUsuario
{
    Guid OwnerId { get; }
}
