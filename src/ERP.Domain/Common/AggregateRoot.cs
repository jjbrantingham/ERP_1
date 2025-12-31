namespace ERP.Domain.Common;

/// <summary>
/// Base class for aggregate roots in the domain model.
/// An aggregate root is the entry point to an aggregate and controls access to all entities within the aggregate boundary.
/// Only aggregate roots can be directly queried from repositories.
/// </summary>
public abstract class AggregateRoot : Entity, IAggregateRoot
{
    // Aggregate roots are entities that serve as the root of an aggregate
    // All access to entities within the aggregate must go through the root
    // This ensures consistency and enforces invariants
}
