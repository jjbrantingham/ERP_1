using ERP.Domain.Common;
using ERP.Domain.PM.ValueObjects;

namespace ERP.Domain.PM.Events;

/// <summary>
/// Domain event raised when a new project is created.
/// </summary>
public class ProjectCreatedEvent : DomainEvent
{
    public long ProjectId { get; }
    public ProjectNumber ProjectNumber { get; }
    public string ProjectName { get; }
    public DateTime CreatedAt { get; }

    public ProjectCreatedEvent(long projectId, ProjectNumber projectNumber, string projectName)
    {
        ProjectId = projectId;
        ProjectNumber = projectNumber;
        ProjectName = projectName;
        CreatedAt = DateTime.UtcNow;
    }
}
