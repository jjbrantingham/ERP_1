using ERP.Application.Common.Interfaces;
using ERP.Domain.WF.Events;
using ERP.Domain.WF.Repositories;
using ERP.Domain.HR.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ERP.Application.WF.EventHandlers;

/// <summary>
/// Sends email notification when a workflow step is activated and requires approval
/// </summary>
public class WorkflowStepActivatedNotificationHandler : INotificationHandler<WorkflowStepActivatedEvent>
{
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<WorkflowStepActivatedNotificationHandler> _logger;

    public WorkflowStepActivatedNotificationHandler(
        IWorkflowInstanceRepository workflowInstanceRepository,
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IEmployeeRepository employeeRepository,
        IEmailService emailService,
        ILogger<WorkflowStepActivatedNotificationHandler> logger)
    {
        _workflowInstanceRepository = workflowInstanceRepository;
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _employeeRepository = employeeRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(WorkflowStepActivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sending notifications for workflow step {StepNumber} activation in workflow {WorkflowInstanceId}",
            notification.StepSequenceNumber,
            notification.WorkflowInstanceId);

        try
        {
            // Get workflow instance with steps
            var workflowInstance = await _workflowInstanceRepository.GetByIdWithStepsAsync(
                notification.WorkflowInstanceId,
                cancellationToken);

            if (workflowInstance == null)
            {
                _logger.LogWarning("Workflow instance {WorkflowInstanceId} not found", notification.WorkflowInstanceId);
                return;
            }

            // Get workflow definition for name
            var workflowDefinition = await _workflowDefinitionRepository.GetByIdAsync(
                workflowInstance.WorkflowDefinitionId,
                cancellationToken);

            // Get the activated step
            var step = workflowInstance.StepInstances.FirstOrDefault(s => s.SequenceNumber == notification.StepSequenceNumber);
            if (step == null)
            {
                _logger.LogWarning("Step {StepNumber} not found in workflow {WorkflowInstanceId}",
                    notification.StepSequenceNumber, notification.WorkflowInstanceId);
                return;
            }

            // Determine recipients based on approver
            var recipients = new List<string>();

            if (step.ApproverId.HasValue)
            {
                // Send to specific approver
                var approver = await _employeeRepository.GetByIdAsync(step.ApproverId.Value, cancellationToken);
                if (approver != null && !string.IsNullOrEmpty(approver.Email))
                {
                    recipients.Add(approver.Email);
                }
            }
            else if (!string.IsNullOrEmpty(step.ApproverRole))
            {
                // TODO: Send to all users with the specified role
                // For now, log that we would send to role
                _logger.LogInformation("Would send notification to role: {Role}", step.ApproverRole);
            }

            // Send email notifications
            foreach (var email in recipients)
            {
                var model = new WorkflowNotificationEmailModel
                {
                    WorkflowName = workflowDefinition?.Name ?? "Workflow",
                    EntityType = workflowInstance.EntityType,
                    EntityId = workflowInstance.EntityId,
                    StepName = step.Name,
                    ActionRequired = "Please review and approve",
                    ActionUrl = $"/workflows/{workflowInstance.Id}",
                    DueDate = step.DeadlineDate ?? DateTime.UtcNow.AddDays(3),
                    RecipientName = email
                };

                await _emailService.SendTemplatedEmailAsync(
                    email,
                    "WorkflowStepActivated",
                    model,
                    cancellationToken);

                _logger.LogInformation("Sent workflow notification to {Email}", email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending workflow notification for step {StepNumber}", notification.StepSequenceNumber);
            // Don't throw - notification failures shouldn't break the workflow
        }
    }
}

/// <summary>
/// Sends email notification when a workflow is completed
/// </summary>
public class WorkflowCompletedNotificationHandler : INotificationHandler<WorkflowInstanceCompletedEvent>
{
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<WorkflowCompletedNotificationHandler> _logger;

    public WorkflowCompletedNotificationHandler(
        IWorkflowInstanceRepository workflowInstanceRepository,
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IEmailService emailService,
        ILogger<WorkflowCompletedNotificationHandler> logger)
    {
        _workflowInstanceRepository = workflowInstanceRepository;
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sending completion notification for workflow {WorkflowInstanceId}",
            notification.WorkflowInstanceId);

        try
        {
            var workflowInstance = await _workflowInstanceRepository.GetByIdWithStepsAsync(
                notification.WorkflowInstanceId,
                cancellationToken);

            if (workflowInstance == null)
                return;

            var workflowDefinition = await _workflowDefinitionRepository.GetByIdAsync(
                workflowInstance.WorkflowDefinitionId,
                cancellationToken);

            // TODO: Determine who should receive completion notification
            // For now, we'll just log
            _logger.LogInformation(
                "Workflow {WorkflowName} completed for {EntityType} #{EntityId}",
                workflowDefinition?.Name,
                notification.EntityType,
                notification.EntityId);

            // Example: Send to workflow initiator, all approvers, etc.
            /*
            var model = new WorkflowNotificationEmailModel
            {
                WorkflowName = workflowDefinition?.Name ?? "Workflow",
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                DueDate = notification.CompletedAt,
                RecipientName = "User"
            };

            await _emailService.SendTemplatedEmailAsync(
                recipientEmail,
                "WorkflowCompleted",
                model,
                cancellationToken);
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending workflow completion notification");
        }
    }
}

/// <summary>
/// Sends email notification when a workflow is rejected
/// </summary>
public class WorkflowRejectedNotificationHandler : INotificationHandler<WorkflowInstanceRejectedEvent>
{
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<WorkflowRejectedNotificationHandler> _logger;

    public WorkflowRejectedNotificationHandler(
        IWorkflowInstanceRepository workflowInstanceRepository,
        IWorkflowDefinitionRepository workflowDefinitionRepository,
        IEmailService emailService,
        ILogger<WorkflowRejectedNotificationHandler> logger)
    {
        _workflowInstanceRepository = workflowInstanceRepository;
        _workflowDefinitionRepository = workflowDefinitionRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceRejectedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sending rejection notification for workflow {WorkflowInstanceId}",
            notification.WorkflowInstanceId);

        try
        {
            var workflowInstance = await _workflowInstanceRepository.GetByIdWithStepsAsync(
                notification.WorkflowInstanceId,
                cancellationToken);

            if (workflowInstance == null)
                return;

            var workflowDefinition = await _workflowDefinitionRepository.GetByIdAsync(
                workflowInstance.WorkflowDefinitionId,
                cancellationToken);

            var rejectedStep = workflowInstance.StepInstances.FirstOrDefault(
                s => s.SequenceNumber == notification.RejectedAtStepNumber);

            // TODO: Determine who should receive rejection notification
            // Typically the workflow initiator
            _logger.LogInformation(
                "Workflow {WorkflowName} rejected at step {StepName} for {EntityType} #{EntityId}",
                workflowDefinition?.Name,
                notification.RejectedAtStepName,
                notification.EntityType,
                notification.EntityId);

            // Example implementation:
            /*
            var model = new WorkflowNotificationEmailModel
            {
                WorkflowName = workflowDefinition?.Name ?? "Workflow",
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                StepName = notification.RejectedAtStepName,
                Comments = rejectedStep?.Comments ?? "No comments provided",
                DueDate = notification.RejectedAt,
                RecipientName = "User"
            };

            await _emailService.SendTemplatedEmailAsync(
                initiatorEmail,
                "WorkflowRejected",
                model,
                cancellationToken);
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending workflow rejection notification");
        }
    }
}
