using ERP.Domain.TE.Repositories;
using ERP.Domain.WF.Events;
using ERP.Application.Common.Interfaces;
using MediatR;

namespace ERP.Application.WF.EventHandlers;

/// <summary>
/// Handles workflow completion events for Timesheets
/// </summary>
public class TimesheetWorkflowCompletedEventHandler : INotificationHandler<WorkflowInstanceCompletedEvent>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TimesheetWorkflowCompletedEventHandler> _logger;

    public TimesheetWorkflowCompletedEventHandler(
        ITimesheetRepository timesheetRepository,
        IUnitOfWork unitOfWork,
        ILogger<TimesheetWorkflowCompletedEventHandler> logger)
    {
        _timesheetRepository = timesheetRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceCompletedEvent notification, CancellationToken cancellationToken)
    {
        // Only handle Timesheet workflows
        if (notification.EntityType != "Timesheet")
            return;

        _logger.LogInformation(
            "Processing workflow completion for Timesheet {EntityId}",
            notification.EntityId);

        // Get the timesheet
        var timesheet = await _timesheetRepository.GetByIdAsync(notification.EntityId, cancellationToken);

        if (timesheet == null)
        {
            _logger.LogWarning(
                "Timesheet {EntityId} not found for workflow completion",
                notification.EntityId);
            return;
        }

        // Approve the timesheet
        timesheet.Approve();

        _logger.LogInformation(
            "Timesheet {TimesheetId} approved via workflow {WorkflowInstanceId}",
            timesheet.Id,
            notification.WorkflowInstanceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handles workflow rejection events for Timesheets
/// </summary>
public class TimesheetWorkflowRejectedEventHandler : INotificationHandler<WorkflowInstanceRejectedEvent>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TimesheetWorkflowRejectedEventHandler> _logger;

    public TimesheetWorkflowRejectedEventHandler(
        ITimesheetRepository timesheetRepository,
        IUnitOfWork unitOfWork,
        ILogger<TimesheetWorkflowRejectedEventHandler> logger)
    {
        _timesheetRepository = timesheetRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceRejectedEvent notification, CancellationToken cancellationToken)
    {
        // Only handle Timesheet workflows
        if (notification.EntityType != "Timesheet")
            return;

        _logger.LogInformation(
            "Processing workflow rejection for Timesheet {EntityId} at step {StepNumber}",
            notification.EntityId,
            notification.RejectedAtStepNumber);

        // Get the timesheet
        var timesheet = await _timesheetRepository.GetByIdAsync(notification.EntityId, cancellationToken);

        if (timesheet == null)
        {
            _logger.LogWarning(
                "Timesheet {EntityId} not found for workflow rejection",
                notification.EntityId);
            return;
        }

        // Reject the timesheet
        timesheet.Reject();

        _logger.LogInformation(
            "Timesheet {TimesheetId} rejected via workflow {WorkflowInstanceId}",
            timesheet.Id,
            notification.WorkflowInstanceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
