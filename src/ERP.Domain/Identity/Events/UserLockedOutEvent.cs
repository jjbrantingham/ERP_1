using ERP.Domain.Common;

namespace ERP.Domain.Identity.Events;

/// <summary>
/// Domain event raised when a user account is locked out due to failed login attempts.
/// </summary>
public class UserLockedOutEvent : DomainEvent
{
    public long UserId { get; }
    public string Email { get; }
    public DateTime LockoutEnd { get; }

    public UserLockedOutEvent(long userId, string email, DateTime lockoutEnd)
    {
        UserId = userId;
        Email = email;
        LockoutEnd = lockoutEnd;
    }
}
