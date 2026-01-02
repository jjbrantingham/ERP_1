using MediatR;

namespace ERP.Application.WF.Commands;

public class StartWorkflowCommand : IRequest<long>
{
    public long WorkflowDefinitionId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public long EntityId { get; init; }
}
