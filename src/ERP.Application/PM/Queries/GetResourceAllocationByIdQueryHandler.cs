using ERP.Application.PM.DTOs;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Handler for GetResourceAllocationByIdQuery.
/// </summary>
public class GetResourceAllocationByIdQueryHandler : IRequestHandler<GetResourceAllocationByIdQuery, ResourceAllocationDto?>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;

    public GetResourceAllocationByIdQueryHandler(IResourceAllocationRepository resourceAllocationRepository)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
    }

    public async Task<ResourceAllocationDto?> Handle(GetResourceAllocationByIdQuery query, CancellationToken cancellationToken)
    {
        var allocation = await _resourceAllocationRepository.GetByIdAsync(query.Id, cancellationToken);

        if (allocation == null)
            return null;

        return new ResourceAllocationDto
        {
            Id = allocation.Id,
            ProjectId = allocation.ProjectId,
            EmployeeId = allocation.EmployeeId,
            StartDate = allocation.StartDate,
            EndDate = allocation.EndDate,
            AllocatedHoursPerWeek = allocation.AllocatedHoursPerWeek,
            Role = allocation.Role,
            Notes = allocation.Notes,
            IsActive = allocation.IsActive,
            CreatedDate = allocation.CreatedDate,
            ModifiedDate = allocation.ModifiedDate
        };
    }
}
