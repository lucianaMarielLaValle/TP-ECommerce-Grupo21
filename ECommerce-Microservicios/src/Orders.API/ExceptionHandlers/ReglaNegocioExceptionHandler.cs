using Microsoft.AspNetCore.Diagnostics;
using Orders.Api.Exceptions;

namespace Orders.API.ExceptionHandlers;

/// <summary>
/// Maneja ReglaNegocioException. El HTTP depende del StatusCode que traiga la excepción:
///   422 → ORD-005 (stock insuficiente)
///   409 → ORD-006 (estado no modificable)
/// </summary>
public class ReglaNegocioExceptionHandler(ILogger<ReglaNegocioExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ReglaNegocioException ex)
            return false;

        var correlationId = context.Items["CorrelationId"]?.ToString();

        logger.LogWarning("[{ErrorCode}] {Message} (CorrelationId: {CorrelationId})",
            ex.ErrorCode, ex.Message, correlationId);

        // type / title / detail según el HTTP del caso (sección 4.3 del enunciado).
        var (type, title, detail) = ex.StatusCode switch
        {
            422 => ("https://tools.ietf.org/html/rfc4918#section-11.2",
                    "Unprocessable Entity",
                    "No se puede procesar la solicitud."),
            _ => ("https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    "Conflict",
                    "No se puede modificar el estado.")
        };

        context.Response.StatusCode = ex.StatusCode;
        await context.Response.WriteAsJsonAsync(new
        {
            type,
            title,
            status = ex.StatusCode,
            detail,
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId
        }, cancellationToken);

        return true;
    }
}
