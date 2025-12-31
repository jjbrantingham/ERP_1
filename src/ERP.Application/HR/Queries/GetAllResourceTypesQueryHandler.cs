using ERP.Application.Common.Interfaces;
using ERP.Application.HR.DTOs;
using ERP.Domain.HR.Repositories;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Handler for GetAllResourceTypesQuery.
/// </summary>
public class GetAllResourceTypesQueryHandler : IQueryHandler<GetAllResourceTypesQuery, IEnumerable<ResourceTypeDto>>
{
    private readonly IResourceTypeRepository _resourceTypeRepository;

    public GetAllResourceTypesQueryHandler(IResourceTypeRepository resourceTypeRepository)
    {
        _resourceTypeRepository = resourceTypeRepository;
    }

    public async Task<IEnumerable<ResourceTypeDto>> Handle(GetAllResourceTypesQuery query, CancellationToken cancellationToken = default)
    {
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
