using Microsoft.AspNetCore.Diagnostics;
using Orders.Api.Exceptions;

namespace Orders.API.ExceptionHandlers;

/// <summary>
/// Maneja NoEncontradoException y responde 404 con el formato Problem Details del enunciado.
/// </summary>
public class NoEncontradoExceptionHandler(ILogger<NoEncontradoExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not NoEncontradoException ex)
            return false;

        var correlationId = context.Items["CorrelationId"]?.ToString();

        // Errores de negocio: nivel Warning (NFR 5.3).
        logger.LogWarning("[{ErrorCode}] {Message} (CorrelationId: {CorrelationId})",
            ex.ErrorCode, ex.Message, correlationId);

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            title = "Not Found",
            status = 404,
            detail = "El recurso solicitado no fue encontrado.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId
        }, cancellationToken);

        return true;
    }
}
