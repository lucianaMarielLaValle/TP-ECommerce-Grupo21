using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/probs/not-found",
            Title = "Recurso no encontrado",
            Status = StatusCodes.Status404NotFound,
            Detail = notFoundException.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = notFoundException.ErrorCode;
        problemDetails.Extensions["errorMessage"] = notFoundException.Message;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}