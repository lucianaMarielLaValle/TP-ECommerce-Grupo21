using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

public class ForbiddenExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ForbiddenException forbiddenException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/probs/forbidden",
            Title = "Acceso prohibido",
            Status = StatusCodes.Status403Forbidden,
            Detail = forbiddenException.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = forbiddenException.ErrorCode;
        problemDetails.Extensions["errorMessage"] = forbiddenException.Message;

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
