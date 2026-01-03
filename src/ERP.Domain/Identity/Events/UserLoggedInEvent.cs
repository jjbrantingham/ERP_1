using ERP.Domain.Common;

namespace ERP.Domain.Identity.Events;

/// <summary>
/// Domain event raised when a user successfully logs in.
/// </summary>
public class UserLoggedInEvent : DomainEvent
{
    public long UserId { get; }
    public string Username { get; }
    public string Email { get; }
    public DateTime LoginTimestamp { get; }

    public UserLoggedInEvent(long userId, string username, string email)
    {
        UserId = userId;
        Username = username;
        Email = email;
        LoginTimestamp = DateTime.UtcNow;
    }
}
