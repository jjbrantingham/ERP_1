using MediatR;

namespace ERP.Application.WF.Commands;

public class DeactivateWorkflowDefinitionCommand : IRequest
{
    public long WorkflowDefinitionId { get; init; }
}
