using ERP.Application.Common.Interfaces;
namespace ERP.Application.PM.DTOs;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get WBS items for a project.
/// </summary>
public class GetProjectWBSItemsQuery : IQuery<IEnumerable<WBSItemDto>
{
    public long ProjectId { get; set; }
}
