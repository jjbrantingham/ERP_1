using ERP.Application.WF.DTOs;
using MediatR;

namespace ERP.Application.WF.Queries;

public class GetPendingApprovalsQuery : IRequest<IEnumerable<WorkflowInstanceDto>>
{
    public long? UserId { get; init; }
    public string? Role { get; init; }
}
