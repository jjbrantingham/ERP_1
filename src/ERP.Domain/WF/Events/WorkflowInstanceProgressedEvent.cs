using ERP.Domain.Common;
using ERP.Domain.WF.Enums;

namespace ERP.Domain.WF.Events;

public class WorkflowInstanceProgressedEvent : DomainEvent
{
    public long WorkflowInstanceId { get; }
    public Guid TenantId { get; }
    public string EntityType { get; }
    public long EntityId { get; }
    public WorkflowInstanceStatus Status { get; }
    public DateTime ProgressedAt { get; }

    public WorkflowInstanceProgressedEvent(
        long workflowInstanceId,
        Guid tenantId,
        string entityType,
        long entityId,
        WorkflowInstanceStatus status)
    {
        WorkflowInstanceId = workflowInstanceId;
        TenantId = tenantId;
        EntityType = entityType;
        EntityId = entityId;
        Status = status;
        ProgressedAt = DateTime.UtcNow;
    }
}
