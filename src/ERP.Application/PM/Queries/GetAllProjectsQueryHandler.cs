using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.PM.DTOs;
using ERP.Domain.PM.Repositories;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Handler for GetAllProjectsQuery.
/// </summary>
public class GetAllProjectsQueryHandler : IQueryHandler<GetAllProjectsQuery, IEnumerable<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetAllProjectsQueryHandler(IProjectRepository projectRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _projectRepository = projectRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<ProjectDto>> Handle(GetAllProjectsQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

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
