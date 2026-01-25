using ERP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="ICurrentTenantService"/> that resolves tenant from HTTP context.
/// </summary>
public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _tenantId;
    private string? _tenantName;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            if (_tenantId.HasValue)
                return _tenantId.Value;

            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                _tenantId = tenantId;
                return tenantId;
            }

            // Check header for tenant ID (useful for API calls)
            var tenantIdHeader = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (tenantIdHeader != null && Guid.TryParse(tenantIdHeader, out var headerTenantId))
            {
                _tenantId = headerTenantId;
                return headerTenantId;
            }

            // Return empty GUID instead of throwing - callers should check IsSet first
            // This prevents exceptions in EF Core query filter evaluation
            return Guid.Empty;
        }
    }

    public string TenantName
    {
        get
        {
            if (_tenantName != null)
                return _tenantName;

            var tenantNameClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantName");
            if (tenantNameClaim != null)
            {
                _tenantName = tenantNameClaim.Value;
                return _tenantName;
            }

            return string.Empty;
        }
    }

    public bool IsSet => _tenantId.HasValue ||
                         _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId") != null ||
                         _httpContextAccessor.HttpContext?.Request.Headers.ContainsKey("X-Tenant-Id") == true;

    public void SetTenant(Guid tenantId, string tenantName)
    {
        _tenantId = tenantId;
        _tenantName = tenantName;
    }
}
