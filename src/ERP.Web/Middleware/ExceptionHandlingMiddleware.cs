using System.Net;
using System.Text.Json;
using ERP.Application.Common.Exceptions;

namespace ERP.Web.Middleware;

/// <summary>
/// Middleware for global exception handling.
/// Catches unhandled exceptions and returns appropriate HTTP responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case NotFoundException notFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = notFoundException.Message;
                errorResponse.StatusCode = response.StatusCode;
                _logger.LogWarning(notFoundException, "Not found: {Message}", notFoundException.Message);
                break;

            case UnauthorizedException unauthorizedException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = unauthorizedException.Message;
                errorResponse.StatusCode = response.StatusCode;
                _logger.LogWarning(unauthorizedException, "Unauthorized: {Message}", unauthorizedException.Message);
                break;

            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Validation failed";
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.Errors = validationException.Errors;
                _logger.LogWarning(validationException, "Validation error: {Errors}", validationException.Errors);
                break;

            case BusinessRuleViolationException businessRuleException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = businessRuleException.Message;
                errorResponse.StatusCode = response.StatusCode;
                errorResponse.RuleName = businessRuleException.RuleName;
                _logger.LogWarning(businessRuleException, "Business rule violation: {RuleName} - {Message}",
                    businessRuleException.RuleName, businessRuleException.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "An internal server error occurred.";
                errorResponse.StatusCode = response.StatusCode;
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        // Add correlation ID for tracking
        if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            errorResponse.CorrelationId = correlationId.ToString();
        }

        errorResponse.Timestamp = DateTime.UtcNow;

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
}

/// <summary>
/// Standard error response model.
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RuleName { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Extension methods for registering exception handling middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
