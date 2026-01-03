using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ERP.Web.Middleware;

/// <summary>
/// Middleware that logs HTTP requests and responses with sanitization of sensitive data
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly HashSet<string> _sensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-API-Key",
        "X-Auth-Token"
    };

    private readonly HashSet<string> _excludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/health/live",
        "/health/ready"
    };

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health check endpoints to reduce noise
        if (_excludedPaths.Contains(context.Request.Path.Value ?? string.Empty))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;

        try
        {
            // Log request
            await LogRequestAsync(context, requestId);

            // Capture original response body stream
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware
            await _next(context);

            stopwatch.Stop();

            // Log response
            await LogResponseAsync(context, requestId, stopwatch.Elapsed);

            // Copy the response to the original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Request failed: {Method} {Path} - {StatusCode} - {Duration}ms - RequestId: {RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId);
            throw;
        }
    }

    private async Task LogRequestAsync(HttpContext context, string requestId)
    {
        var request = context.Request;

        // Build sanitized headers
        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key] = _sensitiveHeaders.Contains(header.Key)
                ? "***REDACTED***"
                : header.Value.ToString();
        }

        // Read request body if present (for POST/PUT/PATCH)
        string? requestBody = null;
        if (request.ContentLength > 0 &&
            (request.Method == HttpMethods.Post ||
             request.Method == HttpMethods.Put ||
             request.Method == HttpMethods.Patch))
        {
            request.EnableBuffering();

            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            requestBody = await reader.ReadToEndAsync();

            // Reset stream position for next middleware
            request.Body.Position = 0;

            // Sanitize request body (remove passwords, tokens, etc.)
            requestBody = SanitizeBody(requestBody);

            // Limit body size in logs to 1000 characters
            if (requestBody.Length > 1000)
            {
                requestBody = requestBody.Substring(0, 1000) + "... [TRUNCATED]";
            }
        }

        _logger.LogInformation(
            "HTTP Request: {Method} {Scheme}://{Host}{Path}{QueryString} - RequestId: {RequestId}, User: {User}, Body: {RequestBody}",
            request.Method,
            request.Scheme,
            request.Host,
            request.Path,
            request.QueryString,
            requestId,
            context.User?.Identity?.Name ?? "Anonymous",
            requestBody ?? "(empty)");
    }

    private async Task LogResponseAsync(HttpContext context, string requestId, TimeSpan duration)
    {
        var response = context.Response;

        // Read response body
        string? responseBody = null;
        if (response.Body.CanRead && response.Body.CanSeek)
        {
            response.Body.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(response.Body, leaveOpen: true);
            responseBody = await reader.ReadToEndAsync();

            response.Body.Seek(0, SeekOrigin.Begin);

            // Limit body size in logs to 1000 characters
            if (responseBody.Length > 1000)
            {
                responseBody = responseBody.Substring(0, 1000) + "... [TRUNCATED]";
            }
        }

        var logLevel = response.StatusCode >= 500
            ? LogLevel.Error
            : response.StatusCode >= 400
                ? LogLevel.Warning
                : LogLevel.Information;

        _logger.Log(logLevel,
            "HTTP Response: {Method} {Path} - {StatusCode} - {Duration}ms - RequestId: {RequestId}, Body: {ResponseBody}",
            context.Request.Method,
            context.Request.Path,
            response.StatusCode,
            duration.TotalMilliseconds,
            requestId,
            responseBody ?? "(empty)");
    }

    private string SanitizeBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return body;

        try
        {
            // Try to parse as JSON and sanitize sensitive fields
            var jsonDoc = JsonDocument.Parse(body);
            var sanitized = SanitizeJsonElement(jsonDoc.RootElement);
            return JsonSerializer.Serialize(sanitized);
        }
        catch
        {
            // If not valid JSON, apply simple string replacement
            var sensitivePatterns = new[]
            {
                "password",
                "passwd",
                "pwd",
                "secret",
                "token",
                "apikey",
                "api_key",
                "authorization",
                "ssn",
                "social_security",
                "credit_card",
                "creditcard",
                "cvv",
                "pin"
            };

            var result = body;
            foreach (var pattern in sensitivePatterns)
            {
                // Case-insensitive replacement of field values
                result = System.Text.RegularExpressions.Regex.Replace(
                    result,
                    $@"(""{pattern}""?\s*[:=]\s*[""']?)([^""',}}\s]+)",
                    "$1***REDACTED***",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            return result;
        }
    }

    private object SanitizeJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new Dictionary<string, object>();
                foreach (var property in element.EnumerateObject())
                {
                    // Sanitize sensitive property values
                    if (IsSensitiveProperty(property.Name))
                    {
                        obj[property.Name] = "***REDACTED***";
                    }
                    else
                    {
                        obj[property.Name] = SanitizeJsonElement(property.Value);
                    }
                }
                return obj;

            case JsonValueKind.Array:
                return element.EnumerateArray().Select(SanitizeJsonElement).ToList();

            case JsonValueKind.String:
                return element.GetString() ?? string.Empty;

            case JsonValueKind.Number:
                return element.GetDecimal();

            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();

            case JsonValueKind.Null:
            default:
                return "null";
        }
    }

    private bool IsSensitiveProperty(string propertyName)
    {
        var sensitiveNames = new[]
        {
            "password",
            "passwd",
            "pwd",
            "secret",
            "token",
            "apikey",
            "api_key",
            "authorization",
            "ssn",
            "social_security_number",
            "credit_card",
            "creditcard",
            "card_number",
            "cvv",
            "pin",
            "securitycode"
        };

        return sensitiveNames.Any(name =>
            propertyName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            propertyName.Contains(name, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Extension methods for registering the request/response logging middleware
/// </summary>
public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
