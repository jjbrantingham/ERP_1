using ERP.Domain.Common;

namespace ERP.Domain.WF.Events;

/// <summary>
/// Domain event raised when a workflow definition is deactivated.
/// </summary>
public class WorkflowDefinitionDeactivatedEvent : DomainEvent
{
    public long WorkflowDefinitionId { get; }
    public Guid TenantId { get; }
    public string Name { get; }
    public DateTime DeactivatedAt { get; }

    public WorkflowDefinitionDeactivatedEvent(long workflowDefinitionId, Guid tenantId, string name)
    {
        WorkflowDefinitionId = workflowDefinitionId;
        TenantId = tenantId;
        Name = name;
        DeactivatedAt = DateTime.UtcNow;
    }
}
