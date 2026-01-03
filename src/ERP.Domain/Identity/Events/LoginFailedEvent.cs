using ERP.Domain.Common;

namespace ERP.Domain.Identity.Events;

/// <summary>
/// Domain event raised when a login attempt fails.
/// </summary>
public class LoginFailedEvent : DomainEvent
{
    public string Username { get; }
    public string Reason { get; }
    public DateTime AttemptTimestamp { get; }

    public LoginFailedEvent(string username, string reason)
    {
        Username = username;
        Reason = reason;
        AttemptTimestamp = DateTime.UtcNow;
    }
}
