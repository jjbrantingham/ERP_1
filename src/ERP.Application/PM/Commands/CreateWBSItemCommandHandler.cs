using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Repositories;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for CreateWBSItemCommand.
/// </summary>
public class CreateWBSItemCommandHandler : ICommandHandler<CreateWBSItemCommand, long>
{
    private readonly IWBSItemRepository _wbsItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateWBSItemCommandHandler(
        IWBSItemRepository wbsItemRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _wbsItemRepository = wbsItemRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateWBSItemCommand command, CancellationToken cancellationToken = default)
    {
        // Create budget if provided
        Money? budget = null;
        if (command.BudgetAmount.HasValue)
        {
            budget = new Money(command.BudgetAmount.Value, command.BudgetCurrency ?? "USD");
        }

        // Create WBS item
        var wbsItem = WBSItem.Create(
            _currentTenant.TenantId,
            command.ProjectId,
            command.WBSCode,
            command.Name,
            command.SortOrder,
            command.ParentId,
            command.Description,
            command.EstimatedHours,
            budget,
            command.StartDate,
            command.EndDate
        );

        await _wbsItemRepository.AddAsync(wbsItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return wbsItem.Id;
    }
}
