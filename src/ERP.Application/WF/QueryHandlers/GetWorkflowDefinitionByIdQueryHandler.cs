using ERP.Application.WF.DTOs;
using ERP.Application.WF.Queries;
using ERP.Domain.WF.Repositories;
using MediatR;
using ERP.Application.Common.Interfaces;

namespace ERP.Application.WF.QueryHandlers;

public class GetWorkflowDefinitionByIdQueryHandler : IRequestHandler<GetWorkflowDefinitionByIdQuery, WorkflowDefinitionDto?>
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetWorkflowDefinitionByIdQueryHandler(IWorkflowDefinitionRepository workflowDefinitionRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<WorkflowDefinitionDto?> Handle(GetWorkflowDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

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
