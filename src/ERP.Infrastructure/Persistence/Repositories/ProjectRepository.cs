using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using ERP.Domain.PM.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Project aggregate root.
/// </summary>
public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<Project?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Project?> GetByProjectNumberAsync(ProjectNumber projectNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.ProjectNumber == projectNumber, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByClientIdAsync(long clientId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.Status == status)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByProjectManagerAsync(long projectManagerId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.ProjectManagerId == projectManagerId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByTypeAsync(ProjectType type, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.ProjectType == type)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetBillableProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.ProjectType == ProjectType.Billable && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var normalizedSearchTerm = searchTerm.ToLower().Trim();

        return await _context.Projects
            .Where(p =>
                p.Name.ToLower().Contains(normalizedSearchTerm) ||
                p.ProjectNumber.Value.ToLower().Contains(normalizedSearchTerm))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .CountAsync(p => p.Status == status, cancellationToken);
    }

    public async Task<bool> ExistsAsync(ProjectNumber projectNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AnyAsync(p => p.ProjectNumber == projectNumber, cancellationToken);
    }
}
