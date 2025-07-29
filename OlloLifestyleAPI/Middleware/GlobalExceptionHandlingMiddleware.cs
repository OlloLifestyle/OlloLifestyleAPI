using System.Net;
using System.Text.Json;
using FluentValidation;

namespace OlloLifestyleAPI.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
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
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogError(ex, "Unhandled exception occurred. CorrelationId: {CorrelationId}, TraceId: {TraceId}, Request: {Method} {Path}", 
                correlationId, context.TraceIdentifier, context.Request.Method, context.Request.Path);
            
            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            CorrelationId = correlationId,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.Message = "Validation failed";
                response.Details = validationEx.Errors.Select(e => new ErrorDetail
                {
                    PropertyName = e.PropertyName,
                    ErrorMessage = e.ErrorMessage,
                    ErrorCode = e.ErrorCode
                }).ToList();
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case KeyNotFoundException:
                response.Message = "Resource not found";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case UnauthorizedAccessException unauthorizedEx:
                response.Message = unauthorizedEx.Message.Contains("Invalid username or password") 
                    ? "Invalid credentials" 
                    : "Access denied";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case ArgumentException argEx:
                response.Message = argEx.Message;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case InvalidOperationException invalidOpEx when invalidOpEx.Message.Contains("tenant"):
                response.Message = "Tenant configuration error";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case TimeoutException:
                response.Message = "Request timeout";
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                break;

            default:
                response.Message = "An internal server error occurred";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                
                // Only include stack trace in development
                if (_environment.IsDevelopment())
                {
                    response.Details.Add(new ErrorDetail
                    {
                        PropertyName = "StackTrace",
                        ErrorMessage = exception.StackTrace ?? "No stack trace available",
                        ErrorCode = "STACK_TRACE"
                    });
                }
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ErrorDetail> Details { get; set; } = new();
    public string CorrelationId { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ErrorDetail
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
}