using MediatR;

namespace ERP.Application.WF.Commands;

public class RejectStepCommand : IRequest
{
    public long WorkflowInstanceId { get; init; }
    public int StepSequenceNumber { get; init; }
    public string Comments { get; init; } = string.Empty;
}
