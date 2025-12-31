using ERP.Domain.Common;

namespace ERP.Domain.Identity.Events;

/// <summary>
/// Domain event raised when a new user is created.
/// </summary>
public class UserCreatedEvent : DomainEvent
{
    public long UserId { get; }
    public string Email { get; }
    public string FullName { get; }

    public UserCreatedEvent(long userId, string email, string fullName)
    {
        UserId = userId;
        Email = email;
        FullName = fullName;
    }
}
