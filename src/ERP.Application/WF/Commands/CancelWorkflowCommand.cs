using MediatR;

namespace ERP.Application.WF.Commands;

public class CancelWorkflowCommand : IRequest
{
    public long WorkflowInstanceId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
