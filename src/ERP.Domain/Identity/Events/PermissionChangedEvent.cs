using ERP.Domain.Common;

namespace ERP.Domain.Identity.Events;

/// <summary>
/// Domain event raised when a user's permissions are changed.
/// </summary>
public class PermissionChangedEvent : DomainEvent
{
    public long TargetUserId { get; }
    public string TargetUsername { get; }
    public string ChangeDescription { get; }
    public DateTime ChangedTimestamp { get; }

    public PermissionChangedEvent(long targetUserId, string targetUsername, string changeDescription)
    {
        TargetUserId = targetUserId;
        TargetUsername = targetUsername;
        ChangeDescription = changeDescription;
        ChangedTimestamp = DateTime.UtcNow;
    }
}
