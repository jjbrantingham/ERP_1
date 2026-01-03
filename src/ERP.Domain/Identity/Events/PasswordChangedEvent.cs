using ERP.Domain.Common;

namespace ERP.Domain.Identity.Events;

/// <summary>
/// Domain event raised when a user's password is changed.
/// </summary>
public class PasswordChangedEvent : DomainEvent
{
    public long UserId { get; }
    public string Username { get; }
    public bool IsReset { get; }
    public DateTime ChangedTimestamp { get; }

    public PasswordChangedEvent(long userId, string username, bool isReset = false)
    {
        UserId = userId;
        Username = username;
        IsReset = isReset;
        ChangedTimestamp = DateTime.UtcNow;
    }
}
