using Microsoft.AspNetCore.Diagnostics;

namespace Products.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error inesperado. ErrorCode={ErrorCode}", "PRD-005");

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Error interno del servidor",
            status = 500,
            detail = "Ocurrió un error inesperado en el servidor.",
            instance = context.Request.Path.Value,
            errorCode = "PRD-005",
            errorMessage = "Error interno al procesar el producto.",
            correlationId = context.Items["CorrelationId"]?.ToString()
        }, cancellationToken);

        return true;
    }
}