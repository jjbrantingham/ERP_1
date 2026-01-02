using ERP.Application.WF.DTOs;
using MediatR;

namespace ERP.Application.WF.Queries;

public class GetWorkflowInstanceByIdQuery : IRequest<WorkflowInstanceDto?>
{
    public long WorkflowInstanceId { get; init; }
}
