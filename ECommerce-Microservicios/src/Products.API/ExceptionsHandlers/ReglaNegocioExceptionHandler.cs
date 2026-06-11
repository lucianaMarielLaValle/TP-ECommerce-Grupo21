using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;

public class ReglaNegocioExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ReglaNegocioException ex) return false;

        context.Response.StatusCode = ex.ErrorCode switch
        {
            "PRD-003" => 409,
            "PRD-004" => 409,
            _ => 400
        };

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
            title = "Conflicto de negocio",
            status = context.Response.StatusCode,
            detail = "La solicitud no se puede procesar por un conflicto de negocio.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId = context.Items["CorrelationId"]?.ToString()
        }, cancellationToken);

        return true;
    }
}