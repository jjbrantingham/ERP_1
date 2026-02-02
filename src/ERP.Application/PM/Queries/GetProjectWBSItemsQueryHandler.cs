using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.PM.DTOs;
using ERP.Domain.PM.Repositories;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Handler for GetProjectWBSItemsQuery.
/// </summary>
public class GetProjectWBSItemsQueryHandler : IRequestHandler<GetProjectWBSItemsQuery, IEnumerable<WBSItemDto>>
{
    private readonly IWBSItemRepository _wbsItemRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetProjectWBSItemsQueryHandler(IWBSItemRepository wbsItemRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _wbsItemRepository = wbsItemRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<WBSItemDto>> Handle(GetProjectWBSItemsQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var wbsItems = await _wbsItemRepository.GetByProjectIdAsync(query.ProjectId, cancellationToken);

        return wbsItems.Select(w => new WBSItemDto
        {
            Id = w.Id,
            TenantId = w.TenantId,
            ProjectId = w.ProjectId,
            ParentId = w.ParentId,
            WBSCode = w.Code,
            Name = w.Name,
            Description = w.Description,
            SortOrder = w.DisplayOrder,
            EstimatedHours = w.EstimatedHours,
            ActualHours = w.ActualHours,
            PercentComplete = w.PercentComplete,
            BudgetAmount = w.Budget?.Amount,
            BudgetCurrency = w.Budget?.Currency,
            StartDate = w.StartDate,
            EndDate = w.EndDate,
            IsActive = w.IsActive,
            CreatedDate = w.CreatedDate,
            ModifiedDate = w.ModifiedDate
        });
    }
}
