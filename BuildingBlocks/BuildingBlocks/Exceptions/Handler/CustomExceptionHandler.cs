using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace  BuildingBlocks.Exceptions.Handler;

public class CustomExceptionHandler(
 ILogger<CustomExceptionHandler> logger   
) : IExceptionHandler
{

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        logger.LogError(
            "Error Message: {exceptionMessage}, Time of occurrence {time}",
            exception.Message, DateTime.UtcNow);
        
        (string Detail, string Title, int StatusCode) details = new();
        details.Detail = exception.Message;
        details.Title = exception.GetType().Name;

        switch (exception)
        {
            case ValidationException:
            case BadRequestException:
                details.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case NotFoundException:
                details.StatusCode = StatusCodes.Status404NotFound;
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                break;

            // case InternalServerException:
            default:
                details.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        var problemDetails = new ProblemDetails{
            Title = details.Title,
            Detail = details.Detail,
            Status = details.StatusCode,
            Instance = context.Request.Path
        };

        problemDetails.Extensions.Add("traceId", context.TraceIdentifier);
        if (exception is ValidationException validationException)
            problemDetails.Extensions.Add("validationErrors", validationException.Errors);

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

}