namespace Hito.Domain.Comun;

// Una regla del dominio impide la operación (por ejemplo, una transición de estado inválida). La API responde 409.
public sealed class ReglaDeNegocioException(string mensaje) : Exception(mensaje);
