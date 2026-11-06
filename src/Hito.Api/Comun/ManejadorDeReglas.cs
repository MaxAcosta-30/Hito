using Hito.Domain.Comun;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Hito.Api.Comun;

// Convierte una regla de negocio violada o un choque de concurrencia en 409 con ProblemDetails.
// Cualquier otra excepción sigue al manejador por defecto (500 sin detalles internos).
public sealed class ManejadorDeReglas(IProblemDetailsService problemas) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception excepcion, CancellationToken ct)
    {
        var titulo = excepcion switch
        {
            ReglaDeNegocioException regla => regla.Message,
            DbUpdateConcurrencyException => "Alguien más cambió este proyecto al mismo tiempo. Recarga e inténtalo de nuevo.",
            _ => null,
        };
        if (titulo is null) return false;

        http.Response.StatusCode = StatusCodes.Status409Conflict;
        return await problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = excepcion,
            ProblemDetails = { Status = StatusCodes.Status409Conflict, Title = titulo },
        });
    }
}
