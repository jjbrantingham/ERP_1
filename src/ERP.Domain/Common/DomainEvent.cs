namespace ERP.Domain.Common;

/// <summary>
/// Base class for domain events.
/// Provides common functionality for all domain events in the system.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEvent"/> class.
    /// </summary>
    protected DomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the date and time when the event occurred (UTC).
    /// </summary>
    public DateTime OccurredOn { get; }
}
