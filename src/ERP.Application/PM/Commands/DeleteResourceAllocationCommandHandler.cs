using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for DeleteResourceAllocationCommand.
/// </summary>
public class DeleteResourceAllocationCommandHandler : IRequestHandler<DeleteResourceAllocationCommand, Unit>
{
    private readonly IResourceAllocationRepository _resourceAllocationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteResourceAllocationCommandHandler(
        IResourceAllocationRepository resourceAllocationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _resourceAllocationRepository = resourceAllocationRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteResourceAllocationCommand command, CancellationToken cancellationToken = default)
    {
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var allocation = await _resourceAllocationRepository.GetByIdAsync(command.Id, cancellationToken);
        if (allocation == null)
            throw new InvalidOperationException($"Resource allocation with ID {command.Id} not found");

        await _resourceAllocationRepository.DeleteAsync(allocation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
