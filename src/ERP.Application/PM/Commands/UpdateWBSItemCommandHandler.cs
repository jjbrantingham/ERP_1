using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for UpdateWBSItemCommand.
/// </summary>
public class UpdateWBSItemCommandHandler : IRequestHandler<UpdateWBSItemCommand, Unit>
{
    private readonly IWBSItemRepository _wbsItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateWBSItemCommandHandler(
        IWBSItemRepository wbsItemRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _wbsItemRepository = wbsItemRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateWBSItemCommand command, CancellationToken cancellationToken = default)
    {
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var wbsItem = await _wbsItemRepository.GetByIdAsync(command.Id, cancellationToken);
        if (wbsItem == null)
            throw new InvalidOperationException($"WBS Item with ID {command.Id} not found");

        Money? budget = null;
        if (command.BudgetAmount.HasValue)
        {
            budget = new Money(command.BudgetAmount.Value, command.BudgetCurrency ?? "USD");
        }

        wbsItem.Update(
            command.Name,
            command.Description,
            command.SortOrder,
            budget,
            command.EstimatedHours,
            command.StartDate,
            command.EndDate
        );

        await _wbsItemRepository.UpdateAsync(wbsItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
