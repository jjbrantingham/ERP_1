using ERP.Application.PM.DTOs;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get all projects.
/// </summary>
public class GetAllProjectsQuery : IRequest<IEnumerable<ProjectDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
