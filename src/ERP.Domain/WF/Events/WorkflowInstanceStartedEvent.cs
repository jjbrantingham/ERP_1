using ERP.Domain.Common;

namespace ERP.Domain.WF.Events;

public class WorkflowInstanceStartedEvent : DomainEvent
{
    public long WorkflowInstanceId { get; }
    public Guid TenantId { get; }
    public long WorkflowDefinitionId { get; }
    public string EntityType { get; }
    public long EntityId { get; }
    public DateTime StartedAt { get; }

    public WorkflowInstanceStartedEvent(
        long workflowInstanceId,
        Guid tenantId,
        long workflowDefinitionId,
        string entityType,
        long entityId)
    {
        WorkflowInstanceId = workflowInstanceId;
        TenantId = tenantId;
        WorkflowDefinitionId = workflowDefinitionId;
        EntityType = entityType;
        EntityId = entityId;
        StartedAt = DateTime.UtcNow;
    }
}
