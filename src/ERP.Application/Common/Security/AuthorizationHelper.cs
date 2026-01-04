using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Interfaces;

namespace ERP.Application.Common.Security;

/// <summary>
/// Helper class for authorization and tenant isolation checks in command/query handlers.
/// </summary>
public static class AuthorizationHelper
{
    /// <summary>
    /// Ensures the current user is authenticated.
    /// </summary>
    /// <param name="currentUser">The current user service.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated.</exception>
    public static void EnsureAuthenticated(ICurrentUserService currentUser)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to perform this operation");
        }
    }

    /// <summary>
    /// Ensures an entity belongs to the current tenant.
    /// </summary>
    /// <param name="entity">The entity to check.</param>
    /// <param name="currentTenant">The current tenant service.</param>
    /// <param name="entityName">The entity name for error messages (optional).</param>
    /// <exception cref="NotFoundException">Thrown when entity is null.</exception>
    /// <exception cref="ForbiddenAccessException">Thrown when entity belongs to a different tenant.</exception>
    public static void EnsureTenantOwnership<TEntity>(
        TEntity? entity,
        ICurrentTenantService currentTenant,
        string? entityName = null) where TEntity : class, ITenantEntity
    {
        if (entity == null)
        {
            throw new NotFoundException(entityName ?? typeof(TEntity).Name, "Entity not found");
        }

        if (entity.TenantId != currentTenant.TenantId)
        {
            throw new ForbiddenAccessException(
                $"Access denied. {entityName ?? typeof(TEntity).Name} belongs to a different tenant");
        }
    }

    /// <summary>
    /// Ensures an entity belongs to the current tenant (async version for repository lookups).
    /// </summary>
    /// <param name="entityId">The entity ID to look up.</param>
    /// <param name="repository">The repository to query.</param>
    /// <param name="currentTenant">The current tenant service.</param>
    /// <param name="entityName">The entity name for error messages (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The entity if it belongs to the current tenant.</returns>
    /// <exception cref="NotFoundException">Thrown when entity is not found.</exception>
    /// <exception cref="ForbiddenAccessException">Thrown when entity belongs to a different tenant.</exception>
    public static async Task<TEntity> EnsureTenantOwnershipAsync<TEntity>(
        long entityId,
        IRepository<TEntity> repository,
        ICurrentTenantService currentTenant,
        string? entityName = null,
        CancellationToken cancellationToken = default) where TEntity : class, ITenantEntity
    {
        var entity = await repository.GetByIdAsync(entityId, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(entityName ?? typeof(TEntity).Name, entityId);
        }

        if (entity.TenantId != currentTenant.TenantId)
        {
            throw new ForbiddenAccessException(
                $"Access denied. {entityName ?? typeof(TEntity).Name} with ID {entityId} belongs to a different tenant");
        }

        return entity;
    }

    /// <summary>
    /// Ensures the current user is authenticated and verifies tenant ownership of an entity.
    /// Combines authentication and tenant checks in one call for convenience.
    /// </summary>
    /// <param name="currentUser">The current user service.</param>
    /// <param name="entity">The entity to check.</param>
    /// <param name="currentTenant">The current tenant service.</param>
    /// <param name="entityName">The entity name for error messages (optional).</param>
    public static void EnsureAuthorizedAndTenantOwnership<TEntity>(
        ICurrentUserService currentUser,
        TEntity? entity,
        ICurrentTenantService currentTenant,
        string? entityName = null) where TEntity : class, ITenantEntity
    {
        EnsureAuthenticated(currentUser);
        EnsureTenantOwnership(entity, currentTenant, entityName);
    }
}
