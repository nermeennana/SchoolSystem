using System;
using DomainLayer.Exceptions;
using Shared.ErrorModels;

namespace SchoolSystem.CustomMiddlewares;

public class CustomExceptionHandlerMiddleware(RequestDelegate _next, ILogger<CustomExceptionHandlerMiddleware> _logger )
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;
        var message = statusCode == StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : exception.Message;

        var errorToReturn = new ErrorToReturn
        {
            StatusCode = statusCode,
            Message = message
        };

        return context.Response.WriteAsJsonAsync(errorToReturn);
    }
}
