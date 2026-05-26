using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Notifications.API.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/probs/internal-error",
            Title = "Error interno",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Ocurrió un error inesperado al procesar la solicitud.",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = "NTF-004";
        problemDetails.Extensions["errorMessage"] = "Error interno del servidor.";

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}