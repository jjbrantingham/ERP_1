using ERP.Application.Common.Interfaces;
namespace ERP.Application.PM.DTOs;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get all projects.
/// </summary>
public class GetAllProjectsQuery : IQuery<IEnumerable<ProjectDto>
{
    public bool ActiveOnly { get; set; } = true;
}
