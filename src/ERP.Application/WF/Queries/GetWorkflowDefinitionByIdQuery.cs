using ERP.Application.WF.DTOs;
using MediatR;

namespace ERP.Application.WF.Queries;

public class GetWorkflowDefinitionByIdQuery : IRequest<WorkflowDefinitionDto?>
{
    public long WorkflowDefinitionId { get; init; }
}
