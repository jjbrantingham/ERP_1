using ERP.Domain.Common;

namespace ERP.Domain.Identity.Events;

/// <summary>
/// Domain event raised when a user's password is changed.
/// </summary>
public class UserPasswordChangedEvent : DomainEvent
{
    public long UserId { get; }
    public string Email { get; }

    public UserPasswordChangedEvent(long userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
