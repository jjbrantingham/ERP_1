using ERP.Application.PM.DTOs;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get all resource allocations for a project.
/// </summary>
public class GetProjectResourceAllocationsQuery : IRequest<IEnumerable<ResourceAllocationDto>>
{
    public long ProjectId { get; init; }
    public bool ActiveOnly { get; init; } = false;
}
