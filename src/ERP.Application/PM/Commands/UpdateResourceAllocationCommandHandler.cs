using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for UpdateResourceAllocationCommand.
/// </summary>
public class UpdateResourceAllocationCommandHandler : IRequestHandler<UpdateResourceAllocationCommand, Unit>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateResourceAllocationCommandHandler(
        IResourceAllocationRepository resourceAllocationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateResourceAllocationCommand command, CancellationToken cancellationToken = default)
    {
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var allocation = await _resourceAllocationRepository.GetByIdAsync(command.Id, cancellationToken);
        if (allocation == null)
            throw new InvalidOperationException($"Resource allocation with ID {command.Id} not found");

        allocation.UpdateAllocation(command.AllocatedHoursPerWeek, command.Role);

        if (command.EndDate.HasValue)
        {
            allocation.End(command.EndDate.Value);
        }

        await _resourceAllocationRepository.UpdateAsync(allocation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
