using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

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
            Type = "https://example.com/probs/business-rule",
            Title = "Regla de negocio violada",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = businessRuleException.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = businessRuleException.ErrorCode;
        problemDetails.Extensions["errorMessage"] = businessRuleException.Message;

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}