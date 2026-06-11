using Microsoft.AspNetCore.Diagnostics;

namespace Orders.API.ExceptionHandlers;

/// <summary>
/// Red de seguridad: cualquier excepción no contemplada cae acá y se responde 500 (ORD-007).
/// Va registrado ÚLTIMO en Program.cs. No expone stack traces; el detalle técnico solo
/// se incluye en entorno Development (NFR 5.2).
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString();

        // Errores inesperados: nivel Error (NFR 5.3).
        logger.LogError(exception, "[ORD-007] Error no controlado (CorrelationId: {CorrelationId})", correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "Ocurrió un error interno al procesar la solicitud.",
            instance = context.Request.Path.Value,
            errorCode = "ORD-007",
            errorMessage = "Error interno al procesar la orden.",
            correlationId,
            // Solo en desarrollo para no filtrar detalles en producción.
            exception = env.IsDevelopment() ? exception.Message : null
        }, cancellationToken);

        return true;
    }
}
