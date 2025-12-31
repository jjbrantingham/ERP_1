using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Repositories;
using ERP.Domain.PM.ValueObjects;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Handler for CreateProjectCommand.
/// </summary>
public class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, long>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateProjectCommand command, CancellationToken cancellationToken = default)
    {
        // Generate project number
        var projectNumber = ProjectNumber.Generate();

        // Ensure uniqueness
        while (await _projectRepository.ExistsAsync(projectNumber, cancellationToken))
        {
            projectNumber = ProjectNumber.Generate();
        }

        // Create budget if provided
        Money? budget = null;
        if (command.BudgetAmount.HasValue)
        {
            budget = new Money(command.BudgetAmount.Value, command.BudgetCurrency ?? "USD");
        }

        // Create project
        var project = Project.Create(
            _currentTenant.TenantId,
            projectNumber,
            command.ClientId,
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

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
