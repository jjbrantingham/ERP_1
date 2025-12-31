using ERP.Application.Common.Interfaces;
using ERP.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Role aggregate.
/// </summary>
public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToUpperInvariant();
        return await _dbSet
            .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.IsSystemRole)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> RoleNameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToUpperInvariant();
        return await _dbSet
            .AnyAsync(r => r.NormalizedName == normalizedName, cancellationToken);
    }
}
