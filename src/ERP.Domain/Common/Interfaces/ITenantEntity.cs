namespace ERP.Domain.Common.Interfaces;

/// <summary>
/// Interface for entities that support multi-tenancy.
/// All entities in the system should implement this interface (via Entity base class).
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// Gets the tenant identifier for this entity.
    /// </summary>
    Guid TenantId { get; }
}
