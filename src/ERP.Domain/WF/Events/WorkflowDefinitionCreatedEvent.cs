using ERP.Domain.Common;

namespace ERP.Domain.WF.Events;

/// <summary>
/// Domain event raised when a new workflow definition is created.
/// </summary>
public class WorkflowDefinitionCreatedEvent : DomainEvent
{
    public long WorkflowDefinitionId { get; }
    public Guid TenantId { get; }
    public string Name { get; }
    public string EntityType { get; }
    public DateTime CreatedAt { get; }

    public WorkflowDefinitionCreatedEvent(long workflowDefinitionId, Guid tenantId, string name, string entityType)
    {
        WorkflowDefinitionId = workflowDefinitionId;
        TenantId = tenantId;
        Name = name;
        EntityType = entityType;
        CreatedAt = DateTime.UtcNow;
    }
}
