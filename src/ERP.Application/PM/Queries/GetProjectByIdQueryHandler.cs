using ERP.Application.Common.Interfaces;
using ERP.Application.PM.DTOs;
using ERP.Domain.PM.Repositories;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Handler for GetProjectByIdQuery.
/// </summary>
public class GetProjectByIdQueryHandler : IQueryHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(query.ProjectId, cancellationToken);

        if (project == null)
            throw new KeyNotFoundException($"Project with ID {query.ProjectId} not found.");

        return new ProjectDto
        {
            Id = project.Id,
            TenantId = project.TenantId,
            ProjectNumber = project.ProjectNumber.Value,
            ClientId = project.ClientId,
            Name = project.Name,
            Description = project.Description,
            ProjectType = project.ProjectType.ToString(),
            BillingMode = project.BillingMode.ToString(),
            Status = project.Status.ToString(),
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            BudgetAmount = project.Budget?.Amount,
            BudgetCurrency = project.Budget?.Currency,
            ProjectManagerId = project.ProjectManagerId,
            Notes = project.Notes,
            IsActive = project.IsActive,
            CreatedDate = project.CreatedDate,
            ModifiedDate = project.ModifiedDate
        };
    }
}
