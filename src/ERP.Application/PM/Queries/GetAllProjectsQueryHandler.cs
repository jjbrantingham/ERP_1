using ERP.Application.Common.Interfaces;
using ERP.Application.PM.DTOs;
using ERP.Domain.PM.Repositories;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Handler for GetAllProjectsQuery.
/// </summary>
public class GetAllProjectsQueryHandler : IQueryHandler<GetAllProjectsQuery, IEnumerable<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;

    public GetAllProjectsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<ProjectDto>> Handle(GetAllProjectsQuery query, CancellationToken cancellationToken = default)
    {
        var projects = query.ActiveOnly
            ? await _projectRepository.GetActiveProjectsAsync(cancellationToken)
            : await _projectRepository.GetAllAsync(cancellationToken);

        return projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            TenantId = p.TenantId,
            ProjectNumber = p.ProjectNumber.Value,
            ClientId = p.ClientId,
            Name = p.Name,
            Description = p.Description,
            ProjectType = p.ProjectType.ToString(),
            BillingMode = p.BillingMode.ToString(),
            Status = p.Status.ToString(),
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            BudgetAmount = p.Budget?.Amount,
            BudgetCurrency = p.Budget?.Currency,
            ProjectManagerId = p.ProjectManagerId,
            Notes = p.Notes,
            IsActive = p.IsActive,
            CreatedDate = p.CreatedDate,
            ModifiedDate = p.ModifiedDate
        });
    }
}
