using System.Security.Principal;
using DigiLoan.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DigiLoan.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        int statusCode;
        string title;
        if (exception is InvalidOperationException)
        {
            statusCode = 400;
            title = "BadRequest";
        }
        else if (exception is ConcurrencyException)
        {
            statusCode = 409;
            title = "DbConcurrencyException";
        }
        else
        {
            statusCode = 500;
            title = "Internal Server Error";
        }

        if (statusCode >= 500)
        {
            _logger.LogError(exception, $"Unhandled exception {exception.Message}");
        }
        else
        {
            _logger.LogWarning($"Handled Exception ({statusCode}): {exception.Message}");
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode < 500 ? exception.Message : "An unexpected error occured",
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}