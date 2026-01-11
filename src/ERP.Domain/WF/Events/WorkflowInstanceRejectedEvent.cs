using ERP.Domain.Common;

namespace ERP.Domain.WF.Events;

public class WorkflowInstanceRejectedEvent : DomainEvent
{
    public long WorkflowInstanceId { get; }
    public Guid TenantId { get; }
    public string EntityType { get; }
    public long EntityId { get; }
    public int RejectedAtStepNumber { get; }
    public string RejectedAtStepName { get; }
    public long RejectedByUserId { get; }
    public string? RejectionReason { get; }
    public DateTime RejectedAt { get; }

    public WorkflowInstanceRejectedEvent(
        long workflowInstanceId,
        Guid tenantId,
        string entityType,
        long entityId,
        int rejectedAtStepNumber,
        string rejectedAtStepName,
        long rejectedByUserId,
        string? rejectionReason)
    {
        WorkflowInstanceId = workflowInstanceId;
        TenantId = tenantId;
        EntityType = entityType;
        EntityId = entityId;
        RejectedAtStepNumber = rejectedAtStepNumber;
        RejectedAtStepName = rejectedAtStepName;
        RejectedByUserId = rejectedByUserId;
        RejectionReason = rejectionReason;
        RejectedAt = DateTime.UtcNow;
    }
}
