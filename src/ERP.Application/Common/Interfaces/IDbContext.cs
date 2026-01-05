using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Database context interface for Application layer.
/// Provides access to DbSets without creating a dependency on Infrastructure.
/// Used for complex queries and operations that span multiple aggregates.
/// </summary>
public interface IDbContext
{
    /// <summary>
    /// Gets the DbSet for the specified entity type.
    /// </summary>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
