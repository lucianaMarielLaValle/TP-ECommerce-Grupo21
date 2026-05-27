using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException businessRuleException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Type = "https://example.com/probs/conflict",
            Title = "Conflicto",
            Status = StatusCodes.Status409Conflict,
            Detail = businessRuleException.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = businessRuleException.ErrorCode;
        problemDetails.Extensions["errorMessage"] = businessRuleException.Message;

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
