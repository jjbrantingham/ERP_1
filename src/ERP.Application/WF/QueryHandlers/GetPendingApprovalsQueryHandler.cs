using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.WF.DTOs;
using ERP.Application.WF.Queries;
using ERP.Domain.WF.Repositories;
using MediatR;

namespace ERP.Application.WF.QueryHandlers;

public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, IEnumerable<WorkflowInstanceDto>>
{
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly ICurrentUserService _currentUser;

    public GetPendingApprovalsQueryHandler(
        IWorkflowInstanceRepository workflowInstanceRepository,
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        ICurrentUserService currentUser)
    {
        _workflowInstanceRepository = workflowInstanceRepository;
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<WorkflowInstanceDto>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        // Use current user if not specified
        var userId = request.UserId ?? _currentUser.UserId;
        if (!userId.HasValue)
            return Enumerable.Empty<WorkflowInstanceDto>();

        var instances = await _workflowInstanceRepository.GetPendingApprovalsForUserAsync(
            userId.Value,
            request.Role,
            cancellationToken);

        var results = new List<WorkflowInstanceDto>();

        foreach (var instance in instances)
        {
            var definition = await _workflowDefinitionRepository.GetByIdAsync(
                instance.WorkflowDefinitionId,
                cancellationToken);

            var currentStep = instance.GetCurrentStep();

            results.Add(new WorkflowInstanceDto
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
            });
        }

        return results;
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
