using Microsoft.AspNetCore.Diagnostics;

namespace Cart.API.ExceptionHandlers;

/// <summary>
/// Red de seguridad
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();
        logger.LogError(exception, "[CRT-005] Error no controlado (CorrelationId: {CorrelationId})", correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "Ocurrió un error interno al procesar la solicitud.",
            instance = context.Request.Path.Value,
            errorCode = "CRT-005",
            errorMessage = "Error interno al procesar el carrito.",
            correlationId,
            exception = env.IsDevelopment() ? exception.Message : null
        }, cancellationToken);

        return true;
    }
}
