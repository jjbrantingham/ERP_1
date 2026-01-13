using ERP.Application.WF.DTOs;
using ERP.Application.WF.Queries;
using ERP.Domain.WF.Repositories;
using MediatR;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;

namespace ERP.Application.WF.QueryHandlers;

public class GetWorkflowInstanceByIdQueryHandler : IRequestHandler<GetWorkflowInstanceByIdQuery, WorkflowInstanceDto?>
{
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetWorkflowInstanceByIdQueryHandler(
        IWorkflowInstanceRepository workflowInstanceRepository,
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _workflowInstanceRepository = workflowInstanceRepository;
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<WorkflowInstanceDto?> Handle(GetWorkflowInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var instance = await _workflowInstanceRepository.GetByIdWithStepsAsync(
            request.WorkflowInstanceId,
            cancellationToken);

        if (instance == null)
            return null;

        // Get workflow definition name
        var definition = await _workflowDefinitionRepository.GetByIdAsync(
            instance.WorkflowDefinitionId,
            cancellationToken);

        var currentStep = instance.GetCurrentStep();

        return new WorkflowInstanceDto
        {
            Id = instance.Id,
            TenantId = instance.TenantId,
            WorkflowDefinitionId = instance.WorkflowDefinitionId,
            WorkflowDefinitionName = definition?.Name ?? "Unknown",
            EntityType = instance.EntityType,
            EntityId = instance.EntityId,
            Status = instance.Status,
            StartedDate = instance.StartedDate,
            CompletedDate = instance.CompletedDate,
            CompletionReason = instance.CompletionReason,
            Steps = instance.StepInstances.Select(s => MapStepInstance(s)).OrderBy(s => s.SequenceNumber).ToList(),
            CurrentStep = currentStep != null ? MapStepInstance(currentStep) : null
        };
    }

    private static StepInstanceDto MapStepInstance(Domain.WF.Entities.StepInstance step)
    {
        return new StepInstanceDto
        {
            Id = step.Id,
            Name = step.Name,
            Description = step.Description,
            StepType = step.StepType,
            SequenceNumber = step.SequenceNumber,
            Status = step.Status,
            ApproverRole = step.ApproverRole,
            ApproverId = step.ApproverId,
            TimeoutHours = step.TimeoutHours,
            ActivatedDate = step.ActivatedDate,
            CompletedDate = step.CompletedDate,
            DeadlineDate = step.DeadlineDate,
            CompletedByUserId = step.CompletedByUserId,
            CompletedByUserName = step.CompletedByUserName,
            Action = step.Action,
            Comments = step.Comments,
            IsOverdue = step.IsOverdue(),
            TimeRemaining = step.GetTimeRemaining()
        };
    }
}
