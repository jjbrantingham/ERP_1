using ERP.Domain.FIN.Repositories;
using ERP.Domain.WF.Events;
using ERP.Application.Common.Interfaces;
using MediatR;

namespace ERP.Application.WF.EventHandlers;

/// <summary>
/// Handles workflow completion events for Journal Entries
/// </summary>
public class JournalEntryWorkflowCompletedEventHandler : INotificationHandler<WorkflowInstanceCompletedEvent>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<JournalEntryWorkflowCompletedEventHandler> _logger;

    public JournalEntryWorkflowCompletedEventHandler(
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<JournalEntryWorkflowCompletedEventHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceCompletedEvent notification, CancellationToken cancellationToken)
    {
        // Only handle JournalEntry workflows
        if (notification.EntityType != "JournalEntry")
            return;

        _logger.LogInformation(
            "Processing workflow completion for JournalEntry {EntityId}",
            notification.EntityId);

        // Get the journal entry
        var journalEntry = await _journalEntryRepository.GetByIdAsync(notification.EntityId, cancellationToken);

        if (journalEntry == null)
        {
            _logger.LogWarning(
                "JournalEntry {EntityId} not found for workflow completion",
                notification.EntityId);
            return;
        }

        // Post the journal entry to the ledger
        var postedBy = _currentUserService.Username ?? "System";
        journalEntry.Post(postedBy);

        _logger.LogInformation(
            "JournalEntry {JournalEntryId} posted to ledger via workflow {WorkflowInstanceId}",
            journalEntry.Id,
            notification.WorkflowInstanceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handles workflow rejection events for Journal Entries
/// </summary>
public class JournalEntryWorkflowRejectedEventHandler : INotificationHandler<WorkflowInstanceRejectedEvent>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JournalEntryWorkflowRejectedEventHandler> _logger;

    public JournalEntryWorkflowRejectedEventHandler(
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork,
        ILogger<JournalEntryWorkflowRejectedEventHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceRejectedEvent notification, CancellationToken cancellationToken)
    {
        // Only handle JournalEntry workflows
        if (notification.EntityType != "JournalEntry")
            return;

        _logger.LogInformation(
            "Processing workflow rejection for JournalEntry {EntityId} at step {StepNumber}",
            notification.EntityId,
            notification.RejectedAtStepNumber);

        // Get the journal entry
        var journalEntry = await _journalEntryRepository.GetByIdAsync(notification.EntityId, cancellationToken);

        if (journalEntry == null)
        {
            _logger.LogWarning(
                "JournalEntry {EntityId} not found for workflow rejection",
                notification.EntityId);
            return;
        }

        // Cancel the journal entry
        journalEntry.Cancel($"Rejected in workflow at step {notification.RejectedAtStepNumber}");

        _logger.LogInformation(
            "JournalEntry {JournalEntryId} cancelled via workflow {WorkflowInstanceId}",
            journalEntry.Id,
            notification.WorkflowInstanceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
