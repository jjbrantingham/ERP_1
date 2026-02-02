using ERP.Application.PM.DTOs;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get WBS items for a project.
/// </summary>
public class GetProjectWBSItemsQuery : IRequest<IEnumerable<WBSItemDto>>
{
    public long ProjectId { get; set; }
}
