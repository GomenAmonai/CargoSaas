using System.Net;
using System.Text.Json;
using Cargo.Core.Exceptions;

namespace Cargo.API.Middleware;

/// <summary>
/// Middleware для глобальной обработки исключений
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Message = exception.Message,
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Errors = validationEx.Errors;
                _logger.LogWarning(exception, "Validation error occurred: {Message}", exception.Message);
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                break;

            case UnauthorizedException unauthorizedEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                _logger.LogWarning(exception, "Unauthorized access attempt: {Message}", exception.Message);
                break;

            case ForbiddenException forbiddenEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                _logger.LogWarning(exception, "Forbidden access attempt: {Message}", exception.Message);
                break;

            case ConflictException conflictEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                _logger.LogWarning(exception, "Conflict occurred: {Message}", exception.Message);
                break;

            case BusinessException businessEx:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                _logger.LogWarning(exception, "Business logic error: {Message}", exception.Message);
                break;

            case CargoException cargoEx:
                context.Response.StatusCode = cargoEx.StatusCode;
                _logger.LogWarning(exception, "Application error: {Message}", exception.Message);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An internal server error occurred. Please try again later.";
                
                // В production не показываем stack trace
                if (_environment.IsDevelopment())
                {
                    response.Details = exception.StackTrace;
                }
                
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                break;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; }
        public string? Details { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }
}
