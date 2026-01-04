using ERP.Domain.Common.Interfaces;

namespace ERP.Domain.Common;

/// <summary>
/// Base class for all entities in the system.
/// Provides identity, audit tracking, and domain event support.
/// </summary>
public abstract class Entity : ITenantEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public long Id { get; protected set; }

    /// <summary>
    /// Gets the tenant identifier for multi-tenancy support.
    /// </summary>
    public Guid TenantId { get; protected set; }

    /// <summary>
    /// Gets the date and time when this entity was created (UTC).
    /// </summary>
    public DateTime CreatedDate { get; protected set; }

    /// <summary>
    /// Gets the user ID who created this entity.
    /// </summary>
    public long? CreatedBy { get; protected set; }

    /// <summary>
    /// Gets the date and time when this entity was last modified (UTC).
    /// </summary>
    public DateTime? ModifiedDate { get; protected set; }

    /// <summary>
    /// Gets the user ID who last modified this entity.
    /// </summary>
    public long? ModifiedBy { get; protected set; }

    /// <summary>
    /// Gets the row version for optimistic concurrency control.
    /// </summary>
    public byte[] RowVersion { get; protected set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Determines whether two entities are equal based on their IDs.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == 0 || other.Id == 0)
            return false;

        return Id == other.Id && TenantId == other.TenantId;
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, TenantId);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}
