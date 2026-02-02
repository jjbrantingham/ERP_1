using ERP.Domain.PM.Enums;
using MediatR;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Command to change a project's status.
/// </summary>
public class ChangeProjectStatusCommand : IRequest
{
    public long ProjectId { get; init; }
    public ProjectStatus NewStatus { get; init; }
}
