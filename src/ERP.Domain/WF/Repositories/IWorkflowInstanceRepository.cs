using ERP.Application.Common.Interfaces;
using ERP.Domain.WF.Entities;
using ERP.Domain.WF.Enums;

namespace ERP.Domain.WF.Repositories;

public interface IWorkflowInstanceRepository : IRepository<WorkflowInstance>
{
    Task<WorkflowInstance?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<WorkflowInstance?> GetByIdWithStepsAsync(long id, CancellationToken cancellationToken = default);
    Task<WorkflowInstance?> GetByEntityAsync(string entityType, long entityId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowInstance>> GetActiveByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowInstance>> GetPendingApprovalsForUserAsync(long userId, string? role = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowInstance>> GetByStatusAsync(WorkflowInstanceStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowInstance>> GetOverdueStepsAsync(CancellationToken cancellationToken = default);
}
