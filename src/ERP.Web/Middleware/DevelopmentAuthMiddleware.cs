using System.Security.Claims;

namespace ERP.Web.Middleware;

/// <summary>
/// Middleware that provides a demo/development authenticated user.
/// Only active when EnableDemoAuth is true in configuration.
/// Sets claims so that CurrentUserService and CurrentTenantService resolve correctly.
/// </summary>
public class DevelopmentAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DevelopmentAuthMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public DevelopmentAuthMiddleware(
        RequestDelegate next,
        ILogger<DevelopmentAuthMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsEnabled())
        {
            await _next(context);
            return;
        }

        // Skip if user is already authenticated (e.g., via JWT or other scheme)
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        var tenantId = _configuration["DefaultTenantId"] ?? "C62F1008-7C13-4F00-B952-DE1F1B202B98";
        var tenantName = _configuration["DefaultTenantName"] ?? "Default Company";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "demouser"),
            new(ClaimTypes.Email, "demo@erp.local"),
            new("TenantId", tenantId),
            new("TenantName", tenantName)
        };

        var identity = new ClaimsIdentity(claims, "DevelopmentAuth");
        context.User = new ClaimsPrincipal(identity);

        _logger.LogDebug("Development auth: request authenticated as demouser (TenantId: {TenantId})", tenantId);

        await _next(context);
    }

    private bool IsEnabled()
    {
        return _configuration.GetValue<bool>("EnableDemoAuth");
    }
}

/// <summary>
/// Extension methods for registering development authentication middleware.
/// </summary>
public static class DevelopmentAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseDevelopmentAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DevelopmentAuthMiddleware>();
    }
}
