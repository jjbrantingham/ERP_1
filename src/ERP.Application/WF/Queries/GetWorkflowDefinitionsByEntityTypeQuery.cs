using ERP.Application.WF.DTOs;
using MediatR;

namespace ERP.Application.WF.Queries;

public class GetWorkflowDefinitionsByEntityTypeQuery : IRequest<IEnumerable<WorkflowDefinitionDto>>
{
    public string EntityType { get; init; } = string.Empty;
    public bool ActiveOnly { get; init; } = true;
}
