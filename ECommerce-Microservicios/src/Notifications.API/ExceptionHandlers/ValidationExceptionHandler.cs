using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/probs/validation",
            Title = "Validación",
            Status = StatusCodes.Status400BadRequest,
            Detail = validationException.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = validationException.ErrorCode;
        problemDetails.Extensions["errorMessage"] = validationException.Message;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}