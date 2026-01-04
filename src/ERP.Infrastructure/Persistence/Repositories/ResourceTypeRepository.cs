using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Repositories;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ResourceType aggregate.
/// </summary>
public class ResourceTypeRepository : Repository<ResourceType>, IResourceTypeRepository
{
    public ResourceTypeRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<ResourceType?> GetByIdWithRatesAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceTypes
            .Include(rt => rt.Rates)
            .FirstOrDefaultAsync(rt => rt.Id == id, cancellationToken);
    }

    public async Task<ResourceType?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceTypes
            .FirstOrDefaultAsync(rt => rt.Name == name, cancellationToken);
    }

    public async Task<ResourceType?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceTypes
            .FirstOrDefaultAsync(rt => rt.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<ResourceType>> GetActiveResourceTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ResourceTypes
            .Where(rt => rt.IsActive)
            .OrderBy(rt => rt.DisplayOrder)
            .ThenBy(rt => rt.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResourceType>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ResourceTypes
            .OrderBy(rt => rt.DisplayOrder)
            .ThenBy(rt => rt.Name)
            .ToListAsync(cancellationToken);
    }
}
