using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
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
    private readonly ICurrentUserService _currentUser;

    public CreateWBSItemCommandHandler(
        IWBSItemRepository wbsItemRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser)
    {
        _wbsItemRepository = wbsItemRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<long> Handle(CreateWBSItemCommand command, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Create budget if provided
        Money? budget = null;
        if (command.BudgetAmount.HasValue)
        {
            budget = new Money(command.BudgetAmount.Value, command.BudgetCurrency ?? "USD");
        }

        // Calculate level from WBS code (count periods + 1)
        var level = command.WBSCode.Split('.').Length;

        // Create WBS item
        var wbsItem = WBSItem.Create(
            _currentTenant.TenantId,
            command.ProjectId,
            command.WBSCode,
            command.Name,
            level,
            command.SortOrder,
            command.ParentId,
            command.Description,
            budget,
            command.EstimatedHours,
            command.StartDate,
            command.EndDate
        );

        await _wbsItemRepository.AddAsync(wbsItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return wbsItem.Id;
    }
}
