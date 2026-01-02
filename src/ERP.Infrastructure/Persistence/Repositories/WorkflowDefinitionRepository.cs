using ERP.Domain.WF.Entities;
using ERP.Domain.WF.Enums;
using ERP.Domain.WF.Repositories;
using ERP.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

public class WorkflowDefinitionRepository : Repository<WorkflowDefinition>, IWorkflowDefinitionRepository
{
    public WorkflowDefinitionRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<WorkflowDefinition?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowDefinitions
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<WorkflowDefinition?> GetByIdWithStepsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetActiveByEntityTypeAsync(
        string entityType,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .Where(w => w.EntityType == entityType && w.Status == WorkflowDefinitionStatus.Active)
            .OrderByDescending(w => w.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetAllByEntityTypeAsync(
        string entityType,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .Where(w => w.EntityType == entityType)
            .OrderByDescending(w => w.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetByStatusAsync(
        WorkflowDefinitionStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowDefinitions
            .Include(w => w.Steps)
            .Where(w => w.Status == status)
            .OrderBy(w => w.EntityType)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }
}
