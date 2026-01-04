using ERP.Domain.Common;

namespace ERP.Domain.Common.Interfaces;

/// <summary>
/// Base repository interface for aggregate roots.
/// Provides common CRUD operations for entities.
/// </summary>
/// <typeparam name="TEntity">The aggregate root type.</typeparam>
public interface IRepository<TEntity> where TEntity : IAggregateRoot
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity with the given ID exists.
    /// </summary>
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of entities.
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
