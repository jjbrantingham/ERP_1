using ERP.Domain.Common;

namespace ERP.Domain.WF.Events;

/// <summary>
/// Domain event raised when a workflow definition is activated.
/// </summary>
public class WorkflowDefinitionActivatedEvent : DomainEvent
{
    public long WorkflowDefinitionId { get; }
    public Guid TenantId { get; }
    public string Name { get; }
    public DateTime ActivatedAt { get; }

    public WorkflowDefinitionActivatedEvent(long workflowDefinitionId, Guid tenantId, string name)
    {
        WorkflowDefinitionId = workflowDefinitionId;
        TenantId = tenantId;
        Name = name;
        ActivatedAt = DateTime.UtcNow;
    }
}
