namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Marker interface for queries.
/// Queries represent read-only operations that return data.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query.</typeparam>
public interface IQuery<out TResult>
{
}
