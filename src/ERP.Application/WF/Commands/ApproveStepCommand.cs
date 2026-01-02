using MediatR;

namespace ERP.Application.WF.Commands;

public class ApproveStepCommand : IRequest
{
    public long WorkflowInstanceId { get; init; }
    public int StepSequenceNumber { get; init; }
    public string? Comments { get; init; }
}
