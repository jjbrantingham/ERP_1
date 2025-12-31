using ERP.Application.Common.Interfaces;
using ERP.Application.HR.DTOs;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Query to get all resource types.
/// </summary>
public class GetAllResourceTypesQuery : IQuery<IEnumerable<ResourceTypeDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
