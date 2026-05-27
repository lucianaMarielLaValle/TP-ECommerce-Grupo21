using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

public class UnauthorizedExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UnauthorizedException unauthorizedException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/probs/unauthorized",
            Title = "No autenticado",
            Status = StatusCodes.Status401Unauthorized,
            Detail = unauthorizedException.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = unauthorizedException.ErrorCode;
        problemDetails.Extensions["errorMessage"] = unauthorizedException.Message;

        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
