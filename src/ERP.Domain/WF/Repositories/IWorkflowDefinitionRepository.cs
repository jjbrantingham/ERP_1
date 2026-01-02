using ERP.Application.Common.Interfaces;
using ERP.Domain.WF.Entities;
using ERP.Domain.WF.Enums;

namespace ERP.Domain.WF.Repositories;

public interface IWorkflowDefinitionRepository : IRepository<WorkflowDefinition>
{
    Task<WorkflowDefinition?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<WorkflowDefinition?> GetByIdWithStepsAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowDefinition>> GetActiveByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowDefinition>> GetAllByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkflowDefinition>> GetByStatusAsync(WorkflowDefinitionStatus status, CancellationToken cancellationToken = default);
}
