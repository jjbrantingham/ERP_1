using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for UpdateProjectCommand.
/// </summary>
public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateProjectCommand command, CancellationToken cancellationToken = default)
    {
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null)
        {
            throw new NotFoundException("Project", command.ProjectId);
        }

        Money? budget = null;
        if (command.BudgetAmount.HasValue)
        {
            budget = new Money(command.BudgetAmount.Value, command.BudgetCurrency ?? "USD");
        }

        project.UpdateInfo(
            command.Name,
            command.ProjectType,
            command.BillingMode,
            command.StartDate,
            command.Description,
            command.EndDate,
            budget,
            command.ProjectManagerId,
            command.Notes
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
