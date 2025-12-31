namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Service for accessing the current tenant context.
/// Provides information about the currently authenticated tenant.
/// </summary>
public interface ICurrentTenantService
{
    /// <summary>
    /// Gets the current tenant ID.
    /// </summary>
    Guid TenantId { get; }

    /// <summary>
    /// Gets the current tenant name.
    /// </summary>
    string TenantName { get; }

    /// <summary>
    /// Gets a value indicating whether a tenant is set.
    /// </summary>
    bool IsSet { get; }

    /// <summary>
    /// Sets the current tenant.
    /// </summary>
    void SetTenant(Guid tenantId, string tenantName);
}
