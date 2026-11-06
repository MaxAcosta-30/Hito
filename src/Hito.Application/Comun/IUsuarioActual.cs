namespace Hito.Application.Comun;

// Id del usuario autenticado, tomado del token. Null si la petición es anónima.
public interface IUsuarioActual
{
    Guid? Id { get; }
}
