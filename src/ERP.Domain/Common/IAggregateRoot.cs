namespace ERP.Domain.Common;

/// <summary>
/// Marker interface for aggregate roots.
/// Aggregate roots are the only entities that can be directly retrieved from repositories.
/// </summary>
public interface IAggregateRoot
{
    // This is a marker interface used to identify aggregate roots
    // It helps ensure that only aggregate roots are exposed through repositories
}
