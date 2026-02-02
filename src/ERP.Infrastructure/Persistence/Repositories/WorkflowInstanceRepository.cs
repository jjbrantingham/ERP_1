using ERP.Domain.Common;
using ERP.Domain.WF.Entities;
using ERP.Domain.WF.Enums;
using ERP.Domain.WF.Repositories;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

public class WorkflowInstanceRepository : Repository<WorkflowInstance>, IWorkflowInstanceRepository
{
    public WorkflowInstanceRepository(ERPDbContext context) : base(context)
    {
    }

    public override async Task<WorkflowInstance?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowInstances
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<WorkflowInstance?> GetByIdWithStepsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowInstances
            .Include(w => w.StepInstances)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<WorkflowInstance?> GetByEntityAsync(
        string entityType,
        long entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowInstances
            .Include(w => w.StepInstances)
            .Where(w => w.EntityType == entityType && w.EntityId == entityId)
            .OrderByDescending(w => w.StartedDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetActiveByEntityTypeAsync(
        string entityType,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowInstances
            .Include(w => w.StepInstances)
            .Where(w => w.EntityType == entityType && w.Status == WorkflowInstanceStatus.InProgress)
            .OrderBy(w => w.StartedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetPendingApprovalsForUserAsync(
        long userId,
        string? role = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WorkflowInstances
            .Include(w => w.StepInstances)
            .Where(w => w.Status == WorkflowInstanceStatus.InProgress);

        // Filter by user or role
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(w => w.StepInstances.Any(s =>
                s.Status == StepInstanceStatus.Active &&
                (s.ApproverId == userId || s.ApproverRole == role)));
        }
        else
        {
            query = query.Where(w => w.StepInstances.Any(s =>
                s.Status == StepInstanceStatus.Active &&
                s.ApproverId == userId));
        }

        return await query
            .OrderBy(w => w.StartedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetByStatusAsync(
        WorkflowInstanceStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.WorkflowInstances
            .Include(w => w.StepInstances)
            .Where(w => w.Status == status)
            .OrderByDescending(w => w.StartedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowInstance>> GetOverdueStepsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.WorkflowInstances
            .Include(w => w.StepInstances)
            .Where(w => w.Status == WorkflowInstanceStatus.InProgress &&
                       w.StepInstances.Any(s => s.Status == StepInstanceStatus.Active &&
                                               s.DeadlineDate.HasValue &&
                                               s.DeadlineDate.Value < now))
            .OrderBy(w => w.StartedDate)
            .ToListAsync(cancellationToken);
    }
}
