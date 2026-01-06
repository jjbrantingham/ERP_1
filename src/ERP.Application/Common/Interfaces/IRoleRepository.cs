using ERP.Domain.Common.Interfaces;
using ERP.Domain.Identity.Entities;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Role aggregate.
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Gets a role by name.
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by ID including permissions.
    /// </summary>
    Task<Role?> GetByIdWithPermissionsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles.
    /// </summary>
    Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all system roles.
    /// </summary>
    Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role name already exists.
    /// </summary>
    Task<bool> RoleNameExistsAsync(string name, CancellationToken cancellationToken = default);
}
