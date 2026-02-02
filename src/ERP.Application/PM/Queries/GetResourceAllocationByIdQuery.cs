using ERP.Application.PM.DTOs;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get a resource allocation by ID.
/// </summary>
public class GetResourceAllocationByIdQuery : IRequest<ResourceAllocationDto?>
{
    public long Id { get; init; }
}
