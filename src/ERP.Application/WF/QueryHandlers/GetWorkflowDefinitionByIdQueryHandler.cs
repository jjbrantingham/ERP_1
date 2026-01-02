using ERP.Application.WF.DTOs;
using ERP.Application.WF.Queries;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.QueryHandlers;

public class GetWorkflowDefinitionByIdQueryHandler : IRequestHandler<GetWorkflowDefinitionByIdQuery, WorkflowDefinitionDto?>
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;

    public GetWorkflowDefinitionByIdQueryHandler(IWorkflowDefinitionRepository workflowDefinitionRepository)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
    }

    public async Task<WorkflowDefinitionDto?> Handle(GetWorkflowDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var workflow = await _workflowDefinitionRepository.GetByIdWithStepsAsync(
            request.WorkflowDefinitionId,
            cancellationToken);

        if (workflow == null)
            return null;

        return new WorkflowDefinitionDto
        {
            Id = workflow.Id,
            TenantId = workflow.TenantId,
            Name = workflow.Name,
            Description = workflow.Description,
            EntityType = workflow.EntityType,
            Status = workflow.Status,
            Version = workflow.Version,
            CreatedDate = workflow.CreatedDate,
            CreatedBy = workflow.CreatedBy,
            Steps = workflow.Steps.Select(s => new WorkflowStepDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                StepType = s.StepType,
                SequenceNumber = s.SequenceNumber,
                ApproverRole = s.ApproverRole,
                ApproverId = s.ApproverId,
                TimeoutHours = s.TimeoutHours,
                Conditions = s.Conditions
            }).OrderBy(s => s.SequenceNumber).ToList()
        };
    }
}
