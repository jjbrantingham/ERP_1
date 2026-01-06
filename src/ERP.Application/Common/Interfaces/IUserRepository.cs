using ERP.Domain.Common.Interfaces;
using ERP.Domain.Identity.Entities;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Repository interface for User aggregate.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by username.
    /// </summary>
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID including their roles.
    /// </summary>
    Task<User?> GetByIdWithRolesAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by username including their roles.
    /// </summary>
    Task<User?> GetByUserNameWithRolesAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users by role.
    /// </summary>
    Task<IEnumerable<User>> GetByRoleAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a username already exists (for uniqueness validation).
    /// </summary>
    Task<bool> UserNameExistsAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email already exists (for uniqueness validation).
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
