using ERP.Application.PM.DTOs;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Handler for GetProjectResourceAllocationsQuery.
/// </summary>
public class GetProjectResourceAllocationsQueryHandler : IRequestHandler<GetProjectResourceAllocationsQuery, IEnumerable<ResourceAllocationDto>>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;

    public GetProjectResourceAllocationsQueryHandler(IResourceAllocationRepository resourceAllocationRepository)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
    }

    public async Task<IEnumerable<ResourceAllocationDto>> Handle(GetProjectResourceAllocationsQuery query, CancellationToken cancellationToken)
    {
        var allocations = query.ActiveOnly
            ? await _resourceAllocationRepository.GetActiveByProjectIdAsync(query.ProjectId, cancellationToken)
            : await _resourceAllocationRepository.GetByProjectIdAsync(query.ProjectId, cancellationToken);

        return allocations.Select(a => new ResourceAllocationDto
        {
            Id = a.Id,
            ProjectId = a.ProjectId,
            EmployeeId = a.EmployeeId,
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            AllocatedHoursPerWeek = a.AllocatedHoursPerWeek,
            Role = a.Role,
            Notes = a.Notes,
            IsActive = a.IsActive,
            CreatedDate = a.CreatedDate,
            ModifiedDate = a.ModifiedDate
        });
    }
}
