using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for CreateResourceAllocationCommand.
/// </summary>
public class CreateResourceAllocationCommandHandler : IRequestHandler<CreateResourceAllocationCommand, long>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;

    public CreateResourceAllocationCommandHandler(
        IResourceAllocationRepository resourceAllocationRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<long> Handle(CreateResourceAllocationCommand command, CancellationToken cancellationToken = default)
    {
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Verify project exists
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {command.ProjectId} not found");

        // Check if employee is already allocated to this project
        var existingAllocation = await _resourceAllocationRepository.ExistsAsync(command.ProjectId, command.EmployeeId, cancellationToken);
        if (existingAllocation)
            throw new InvalidOperationException($"Employee {command.EmployeeId} is already allocated to project {command.ProjectId}");

        var resourceAllocation = ResourceAllocation.Create(
            _currentTenant.TenantId,
            command.ProjectId,
            command.EmployeeId,
            command.StartDate,
            command.AllocatedHoursPerWeek,
            command.Role,
            command.EndDate,
            command.Notes
        );

        await _resourceAllocationRepository.AddAsync(resourceAllocation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return resourceAllocation.Id;
    }
}
