using ERP.Domain.Common.Interfaces;
using ERP.Domain.HR.Entities;

namespace ERP.Domain.HR.Repositories;

/// <summary>
/// Repository interface for ResourceType aggregate.
/// </summary>
public interface IResourceTypeRepository : IRepository<ResourceType>
{
    /// <summary>
    /// Gets a resource type by ID with rates included.
    /// </summary>
    Task<ResourceType?> GetByIdWithRatesAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a resource type by name.
    /// </summary>
    Task<ResourceType?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a resource type by code.
    /// </summary>
    Task<ResourceType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active resource types.
    /// </summary>
    Task<IEnumerable<ResourceType>> GetActiveResourceTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all resource types ordered by display order.
    /// </summary>
    Task<IEnumerable<ResourceType>> GetAllOrderedAsync(CancellationToken cancellationToken = default);
}
