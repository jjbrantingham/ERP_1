using MediatR;

namespace ERP.Application.WF.Commands;

public class ActivateWorkflowDefinitionCommand : IRequest
{
    public long WorkflowDefinitionId { get; init; }
}
