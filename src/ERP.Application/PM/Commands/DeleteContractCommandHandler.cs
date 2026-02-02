using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for DeleteContractCommand.
/// </summary>
public class DeleteContractCommandHandler : IRequestHandler<DeleteContractCommand, Unit>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteContractCommandHandler(
        IContractRepository contractRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteContractCommand command, CancellationToken cancellationToken = default)
    {
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var contract = await _contractRepository.GetByIdAsync(command.Id, cancellationToken);
        if (contract == null)
            throw new InvalidOperationException($"Contract with ID {command.Id} not found");

        await _contractRepository.DeleteAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
