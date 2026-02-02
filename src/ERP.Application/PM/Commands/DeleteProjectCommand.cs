using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to delete (deactivate) a project.
/// </summary>
public class DeleteProjectCommand : IRequest
{
    public long ProjectId { get; init; }
}
