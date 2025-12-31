using ERP.Application.Common.Interfaces;

namespace ERP.Web.Middleware;

/// <summary>
/// Middleware for resolving the current tenant from the request.
/// Supports multiple resolution strategies: subdomain, header, and claim.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        Guid? tenantId = null;
        string? tenantName = null;

        // Strategy 1: Try to get tenant from claims (if user is authenticated)
        var tenantIdClaim = context.User?.FindFirst("TenantId");
        if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var claimTenantId))
        {
            tenantId = claimTenantId;
            tenantName = context.User?.FindFirst("TenantName")?.Value ?? "Unknown";
            _logger.LogDebug("Tenant resolved from claims: {TenantId}", tenantId);
        }

        // Strategy 2: Try to get tenant from header (for API calls)
        if (!tenantId.HasValue)
        {
            var tenantIdHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (tenantIdHeader != null && Guid.TryParse(tenantIdHeader, out var headerTenantId))
            {
                tenantId = headerTenantId;
                tenantName = context.Request.Headers["X-Tenant-Name"].FirstOrDefault() ?? "Unknown";
                _logger.LogDebug("Tenant resolved from header: {TenantId}", tenantId);
            }
        }

        // Strategy 3: Try to get tenant from subdomain
        if (!tenantId.HasValue)
        {
            var host = context.Request.Host.Host;
            var parts = host.Split('.');

            if (parts.Length >= 2)
            {
                var subdomain = parts[0];
                // TODO: Look up tenant ID from subdomain in database
                // For now, log that we found a subdomain
                _logger.LogDebug("Subdomain found: {Subdomain}", subdomain);
            }
        }

        // Set the tenant in the service if found
        if (tenantId.HasValue)
        {
            tenantService.SetTenant(tenantId.Value, tenantName ?? "Unknown");
        }
        else
        {
            // For development/testing, you might want to set a default tenant
            // Or require tenant for all requests
            _logger.LogWarning("No tenant ID found in request");
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for registering tenant resolution middleware.
/// </summary>
public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantResolutionMiddleware>();
    }
}
