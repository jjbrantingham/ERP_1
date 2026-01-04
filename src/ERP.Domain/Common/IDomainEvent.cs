using MediatR;

namespace ERP.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something significant that happened in the domain.
/// Extends INotification to enable MediatR integration.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the date and time when the event occurred (UTC).
    /// </summary>
    DateTime OccurredOn { get; }
}
