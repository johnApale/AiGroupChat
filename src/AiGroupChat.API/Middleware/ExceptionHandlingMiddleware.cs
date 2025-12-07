using System.Text.Json;
using AiGroupChat.Application.Exceptions;

namespace AiGroupChat.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            AuthenticationException ex => (
                StatusCodes.Status401Unauthorized,
                new ErrorResponse("AuthenticationError", ex.Message)
            ),
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse("ValidationError", ex.Message, ex.Errors)
            ),
            NotFoundException ex => (
                StatusCodes.Status404NotFound,
                new ErrorResponse("NotFound", ex.Message)
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ErrorResponse("InternalError", "An unexpected error occurred.")
            )
        };

        // Log the exception
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
        }
        else
        {
            _logger.LogWarning(exception, "A handled exception occurred: {Message}", exception.Message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, jsonOptions);

        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public string Error { get; set; }
    public string Message { get; set; }
    public string[]? Details { get; set; }

    public ErrorResponse(string error, string message, string[]? details = null)
    {
        Error = error;
        Message = message;
        Details = details;
    }
}