using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Repositories;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for CreateContractCommand.
/// </summary>
public class CreateContractCommandHandler : ICommandHandler<CreateContractCommand, long>
{
    private readonly IContractRepository _contractRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateContractCommandHandler(
        IContractRepository contractRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _contractRepository = contractRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateContractCommand command, CancellationToken cancellationToken = default)
    {
        // Create contract value if provided
        Money? contractValue = null;
        if (command.ContractValueAmount.HasValue)
        {
            contractValue = new Money(command.ContractValueAmount.Value, command.ContractValueCurrency ?? "USD");
        }

        // Create contract
        var contract = Contract.Create(
            _currentTenant.TenantId,
            command.ProjectId,
            command.ContractNumber,
            command.ContractType,
            command.StartDate,
            command.EndDate,
            command.Title,
            command.Description,
            contractValue,
            command.SignedDate,
            command.Terms
        );

        await _contractRepository.AddAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return contract.Id;
    }
}
