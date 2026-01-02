using ERP.Domain.Common;

namespace ERP.Domain.WF.Events;

public class WorkflowStepActivatedEvent : DomainEvent
{
    public long WorkflowInstanceId { get; }
    public Guid TenantId { get; }
    public int StepSequenceNumber { get; }
    public string StepName { get; }
    public DateTime ActivatedAt { get; }

    public WorkflowStepActivatedEvent(
        long workflowInstanceId,
        Guid tenantId,
        int stepSequenceNumber,
        string stepName)
    {
        WorkflowInstanceId = workflowInstanceId;
        TenantId = tenantId;
        StepSequenceNumber = stepSequenceNumber;
        StepName = stepName;
        ActivatedAt = DateTime.UtcNow;
    }
}
