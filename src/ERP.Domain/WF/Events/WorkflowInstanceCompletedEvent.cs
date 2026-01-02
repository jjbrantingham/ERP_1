using ERP.Domain.Common;

namespace ERP.Domain.WF.Events;

public class WorkflowInstanceCompletedEvent : DomainEvent
{
    public long WorkflowInstanceId { get; }
    public Guid TenantId { get; }
    public string EntityType { get; }
    public long EntityId { get; }
    public DateTime CompletedAt { get; }

    public WorkflowInstanceCompletedEvent(
        long workflowInstanceId,
        Guid tenantId,
        string entityType,
        long entityId,
        DateTime completedAt)
    {
        WorkflowInstanceId = workflowInstanceId;
        TenantId = tenantId;
        EntityType = entityType;
        EntityId = entityId;
        CompletedAt = completedAt;
    }
}
