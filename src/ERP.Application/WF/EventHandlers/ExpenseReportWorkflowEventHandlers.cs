using ERP.Domain.TE.Repositories;
using ERP.Domain.WF.Events;
using ERP.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ERP.Application.WF.EventHandlers;

/// <summary>
/// Handles workflow completion events for Expense Reports
/// </summary>
public class ExpenseReportWorkflowCompletedEventHandler : INotificationHandler<WorkflowInstanceCompletedEvent>
{
    private readonly IExpenseReportRepository _expenseReportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpenseReportWorkflowCompletedEventHandler> _logger;

    public ExpenseReportWorkflowCompletedEventHandler(
        IExpenseReportRepository expenseReportRepository,
        IUnitOfWork unitOfWork,
        ILogger<ExpenseReportWorkflowCompletedEventHandler> logger)
    {
        _expenseReportRepository = expenseReportRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceCompletedEvent notification, CancellationToken cancellationToken)
    {
        // Only handle ExpenseReport workflows
        if (notification.EntityType != "ExpenseReport")
            return;

        _logger.LogInformation(
            "Processing workflow completion for ExpenseReport {EntityId}",
            notification.EntityId);

        // Get the expense report
        var expenseReport = await _expenseReportRepository.GetByIdAsync(notification.EntityId, cancellationToken);

        if (expenseReport == null)
        {
            _logger.LogWarning(
                "ExpenseReport {EntityId} not found for workflow completion",
                notification.EntityId);
            return;
        }

        // Approve the expense report
        expenseReport.Approve();

        _logger.LogInformation(
            "ExpenseReport {ExpenseReportId} approved via workflow {WorkflowInstanceId}",
            expenseReport.Id,
            notification.WorkflowInstanceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handles workflow rejection events for Expense Reports
/// </summary>
public class ExpenseReportWorkflowRejectedEventHandler : INotificationHandler<WorkflowInstanceRejectedEvent>
{
    private readonly IExpenseReportRepository _expenseReportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpenseReportWorkflowRejectedEventHandler> _logger;

    public ExpenseReportWorkflowRejectedEventHandler(
        IExpenseReportRepository expenseReportRepository,
        IUnitOfWork unitOfWork,
        ILogger<ExpenseReportWorkflowRejectedEventHandler> logger)
    {
        _expenseReportRepository = expenseReportRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceRejectedEvent notification, CancellationToken cancellationToken)
    {
        // Only handle ExpenseReport workflows
        if (notification.EntityType != "ExpenseReport")
            return;

        _logger.LogInformation(
            "Processing workflow rejection for ExpenseReport {EntityId} at step {StepNumber}",
            notification.EntityId,
            notification.RejectedAtStepNumber);

        // Get the expense report
        var expenseReport = await _expenseReportRepository.GetByIdAsync(notification.EntityId, cancellationToken);

        if (expenseReport == null)
        {
            _logger.LogWarning(
                "ExpenseReport {EntityId} not found for workflow rejection",
                notification.EntityId);
            return;
        }

        // Reject the expense report
        expenseReport.Reject();

        _logger.LogInformation(
            "ExpenseReport {ExpenseReportId} rejected via workflow {WorkflowInstanceId}",
            expenseReport.Id,
            notification.WorkflowInstanceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
