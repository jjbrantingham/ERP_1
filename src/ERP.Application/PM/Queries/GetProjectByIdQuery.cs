using ERP.Application.PM.DTOs;
using MediatR;

namespace ERP.Application.PM.Queries;

/// <summary>
/// Query to get project by ID.
/// </summary>
public class GetProjectByIdQuery : IRequest<ProjectDto>
{
    public long ProjectId { get; set; }
}
