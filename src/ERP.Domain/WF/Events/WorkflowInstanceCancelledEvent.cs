using ERP.Domain.Common;

namespace ERP.Domain.WF.Events;

public class WorkflowInstanceCancelledEvent : DomainEvent
{
    public long WorkflowInstanceId { get; }
    public Guid TenantId { get; }
    public string EntityType { get; }
    public long EntityId { get; }
    public string Reason { get; }
    public DateTime CancelledAt { get; }

    public WorkflowInstanceCancelledEvent(
        long workflowInstanceId,
        Guid tenantId,
        string entityType,
        long entityId,
        string reason)
    {
        WorkflowInstanceId = workflowInstanceId;
        TenantId = tenantId;
        EntityType = entityType;
        EntityId = entityId;
        Reason = reason;
        CancelledAt = DateTime.UtcNow;
    }
}
