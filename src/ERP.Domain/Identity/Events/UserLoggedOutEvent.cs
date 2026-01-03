using ERP.Domain.Common;

namespace ERP.Domain.Identity.Events;

/// <summary>
/// Domain event raised when a user logs out.
/// </summary>
public class UserLoggedOutEvent : DomainEvent
{
    public long UserId { get; }
    public string Username { get; }
    public DateTime LogoutTimestamp { get; }

    public UserLoggedOutEvent(long userId, string username)
    {
        UserId = userId;
        Username = username;
        LogoutTimestamp = DateTime.UtcNow;
    }
}
