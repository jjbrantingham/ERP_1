using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for WBS Item entity.
/// </summary>
public class WBSItemRepository : Repository<WBSItem>, IWBSItemRepository
{
    public WBSItemRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<WBSItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.WBSItems
            .Include(w => w.Project)
            .Include(w => w.Parent)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WBSItem>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _context.WBSItems
            .Where(w => w.ProjectId == projectId)
            .OrderBy(w => w.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WBSItem>> GetByParentIdAsync(long parentId, CancellationToken cancellationToken = default)
    {
        return await _context.WBSItems
            .Where(w => w.ParentId == parentId)
            .OrderBy(w => w.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WBSItem>> GetRootItemsAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _context.WBSItems
            .Where(w => w.ProjectId == projectId && w.ParentId == null)
            .OrderBy(w => w.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<WBSItem?> GetByWBSCodeAsync(long projectId, string wbsCode, CancellationToken cancellationToken = default)
    {
        return await _context.WBSItems
            .FirstOrDefaultAsync(w => w.ProjectId == projectId && w.WBSCode == wbsCode, cancellationToken);
    }

    public async Task<IEnumerable<WBSItem>> GetHierarchicalTreeAsync(long projectId, CancellationToken cancellationToken = default)
    {
        // Load all WBS items for the project with parent relationships
        return await _context.WBSItems
            .Where(w => w.ProjectId == projectId)
            .Include(w => w.Parent)
            .OrderBy(w => w.WBSCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(long projectId, string wbsCode, CancellationToken cancellationToken = default)
    {
        return await _context.WBSItems
            .AnyAsync(w => w.ProjectId == projectId && w.WBSCode == wbsCode, cancellationToken);
    }
}
