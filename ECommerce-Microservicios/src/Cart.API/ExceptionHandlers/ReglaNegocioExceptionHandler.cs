using Microsoft.AspNetCore.Diagnostics;
using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

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

        var (type, title, detail) = ex.StatusCode switch
        {
            422 => ("https://tools.ietf.org/html/rfc4918#section-11.2",
                    "Unprocessable Entity",
                    "No se puede procesar la solicitud."),
            _ => ("https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    "Conflict",
                    "No se puede completar la operación.")
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
