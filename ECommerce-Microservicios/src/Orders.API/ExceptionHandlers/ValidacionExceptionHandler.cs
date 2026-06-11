using Microsoft.AspNetCore.Diagnostics;
using Orders.Api.Exceptions;

namespace Orders.Api.ExceptionHandlers;

/// <summary>
/// Maneja ValidacionException y responde 400 con el formato Problem Details del enunciado.
/// </summary>
public class ValidacionExceptionHandler(ILogger<ValidacionExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidacionException ex)
            return false;

        var correlationId = context.Items["CorrelationId"]?.ToString();

        logger.LogWarning("[{ErrorCode}] {Message} (CorrelationId: {CorrelationId})",
            ex.ErrorCode, ex.Message, correlationId);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Bad Request",
            status = 400,
            detail = "La solicitud contiene datos inválidos.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId
        }, cancellationToken);

        return true;
    }
}
