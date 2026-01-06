using ERP.Application.Common.Interfaces;
using ERP.Application.PM.DTOs;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get project by ID.
/// </summary>
public class GetProjectByIdQuery : IQuery<ProjectDto>
{
    public long ProjectId { get; set; }
}
