using ERP.Application.WF.DTOs;
using MediatR;

namespace ERP.Application.WF.Queries;

public class GetWorkflowInstanceByEntityQuery : IRequest<WorkflowInstanceDto?>
{
    public string EntityType { get; init; } = string.Empty;
    public long EntityId { get; init; }
}
