using ERP.Application.WF.DTOs;
using ERP.Application.WF.Queries;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.QueryHandlers;

public class GetWorkflowDefinitionsByEntityTypeQueryHandler
    : IRequestHandler<GetWorkflowDefinitionsByEntityTypeQuery, IEnumerable<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;

    public GetWorkflowDefinitionsByEntityTypeQueryHandler(IWorkflowDefinitionRepository workflowDefinitionRepository)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
    }

    public async Task<IEnumerable<WorkflowDefinitionDto>> Handle(
        GetWorkflowDefinitionsByEntityTypeQuery request,
        CancellationToken cancellationToken)
    {
        var workflows = request.ActiveOnly
            ? await _workflowDefinitionRepository.GetActiveByEntityTypeAsync(request.EntityType, cancellationToken)
            : await _workflowDefinitionRepository.GetAllByEntityTypeAsync(request.EntityType, cancellationToken);

        return workflows.Select(w => new WorkflowDefinitionDto
        {
            Id = w.Id,
            TenantId = w.TenantId,
            Name = w.Name,
            Description = w.Description,
            EntityType = w.EntityType,
            Status = w.Status,
            Version = w.Version,
            CreatedDate = w.CreatedDate,
            CreatedBy = w.CreatedBy,
            Steps = w.Steps.Select(s => new WorkflowStepDto
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
        }).ToList();
    }
}
