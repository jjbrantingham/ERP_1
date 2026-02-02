using MediatR;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Handler interface for queries.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResult">The type of result returned.</typeparam>
public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IQuery<TResult>, IRequest<TResult>
{
}
