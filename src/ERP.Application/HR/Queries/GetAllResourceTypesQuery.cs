using ERP.Application.HR.DTOs;
using MediatR;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Query to get all resource types.
/// </summary>
public class GetAllResourceTypesQuery : IRequest<IEnumerable<ResourceTypeDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
