using ERP.Domain.Common;

namespace ERP.Domain.WF.Events;

public class WorkflowStepReturnedEvent : DomainEvent
{
    public long WorkflowInstanceId { get; }
    public Guid TenantId { get; }
    public string EntityType { get; }
    public long EntityId { get; }
    public int StepSequenceNumber { get; }
    public string StepName { get; }
    public DateTime ReturnedAt { get; }

    public WorkflowStepReturnedEvent(
        long workflowInstanceId,
        Guid tenantId,
        string entityType,
        long entityId,
        int stepSequenceNumber,
        string stepName)
    {
        WorkflowInstanceId = workflowInstanceId;
        TenantId = tenantId;
        EntityType = entityType;
        EntityId = entityId;
        StepSequenceNumber = stepSequenceNumber;
        StepName = stepName;
        ReturnedAt = DateTime.UtcNow;
    }
}
