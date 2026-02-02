using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.HR.DTOs;
using ERP.Domain.HR.Repositories;
using MediatR;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Handler for GetAllResourceTypesQuery.
/// </summary>
public class GetAllResourceTypesQueryHandler : IRequestHandler<GetAllResourceTypesQuery, IEnumerable<ResourceTypeDto>>
{
    private readonly IResourceTypeRepository _resourceTypeRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetAllResourceTypesQueryHandler(IResourceTypeRepository resourceTypeRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _resourceTypeRepository = resourceTypeRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<ResourceTypeDto>> Handle(GetAllResourceTypesQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var resourceTypes = query.ActiveOnly
            ? await _resourceTypeRepository.GetActiveResourceTypesAsync(cancellationToken)
            : await _resourceTypeRepository.GetAllOrderedAsync(cancellationToken);

        return resourceTypes.Select(rt => new ResourceTypeDto
        {
            Id = rt.Id,
            Name = rt.Name,
            Description = rt.Description,
            Code = rt.Code,
            IsActive = rt.IsActive,
            DisplayOrder = rt.DisplayOrder,
            CreatedDate = rt.CreatedDate,
            ModifiedDate = rt.ModifiedDate
        });
    }
}
