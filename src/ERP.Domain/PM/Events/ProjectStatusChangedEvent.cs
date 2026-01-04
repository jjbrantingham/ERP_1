using ERP.Domain.Common;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.ValueObjects;

namespace ERP.Domain.PM.Events;

/// <summary>
/// Domain event raised when a project's status changes.
/// </summary>
public class ProjectStatusChangedEvent : DomainEvent
{
    public long ProjectId { get; }
    public ProjectNumber ProjectNumber { get; }
    public string ProjectName { get; }
    public ProjectStatus OldStatus { get; }
    public ProjectStatus NewStatus { get; }
    public DateTime ChangedAt { get; }

    public ProjectStatusChangedEvent(
        long projectId,
        ProjectNumber projectNumber,
        string projectName,
        ProjectStatus oldStatus,
        ProjectStatus newStatus)
    {
        ProjectId = projectId;
        ProjectNumber = projectNumber;
        ProjectName = projectName;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = DateTime.UtcNow;
    }
}
