using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for UpdateContractCommand.
/// </summary>
public class UpdateContractCommandHandler : IRequestHandler<UpdateContractCommand, Unit>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateContractCommandHandler(
        IContractRepository contractRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateContractCommand command, CancellationToken cancellationToken = default)
    {
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var contract = await _contractRepository.GetByIdAsync(command.Id, cancellationToken);
        if (contract == null)
            throw new InvalidOperationException($"Contract with ID {command.Id} not found");

        Money? contractValue = null;
        if (command.ContractValueAmount.HasValue)
        {
            contractValue = new Money(command.ContractValueAmount.Value, command.ContractValueCurrency ?? "USD");
        }

        contract.Update(
            command.Title,
            command.Description,
            contractValue,
            command.StartDate,
            command.EndDate,
            command.SignedDate,
            command.Terms
        );

        await _contractRepository.UpdateAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
