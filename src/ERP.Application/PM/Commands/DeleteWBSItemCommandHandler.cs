using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for DeleteWBSItemCommand.
/// </summary>
public class DeleteWBSItemCommandHandler : IRequestHandler<DeleteWBSItemCommand, Unit>
{
    private readonly IWBSItemRepository _wbsItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteWBSItemCommandHandler(
        IWBSItemRepository wbsItemRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _wbsItemRepository = wbsItemRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteWBSItemCommand command, CancellationToken cancellationToken = default)
    {
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var wbsItem = await _wbsItemRepository.GetByIdAsync(command.Id, cancellationToken);
        if (wbsItem == null)
            throw new InvalidOperationException($"WBS Item with ID {command.Id} not found");

        await _wbsItemRepository.DeleteAsync(wbsItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
