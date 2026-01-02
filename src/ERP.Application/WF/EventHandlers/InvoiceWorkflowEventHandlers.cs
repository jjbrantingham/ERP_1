using ERP.Domain.BILL.Repositories;
using ERP.Domain.WF.Events;
using ERP.Application.Common.Interfaces;
using MediatR;

namespace ERP.Application.WF.EventHandlers;

/// <summary>
/// Handles workflow completion events for Invoices
/// </summary>
public class InvoiceWorkflowCompletedEventHandler : INotificationHandler<WorkflowInstanceCompletedEvent>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvoiceWorkflowCompletedEventHandler> _logger;

    public InvoiceWorkflowCompletedEventHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<InvoiceWorkflowCompletedEventHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceCompletedEvent notification, CancellationToken cancellationToken)
    {
        // Only handle Invoice workflows
        if (notification.EntityType != "Invoice")
            return;

        _logger.LogInformation(
            "Processing workflow completion for Invoice {EntityId}",
            notification.EntityId);

        // Get the invoice
        var invoice = await _invoiceRepository.GetByIdAsync(notification.EntityId, cancellationToken);

        if (invoice == null)
        {
            _logger.LogWarning(
                "Invoice {EntityId} not found for workflow completion",
                notification.EntityId);
            return;
        }

        // Post the invoice (makes it official and creates accounting entries)
        invoice.Post();

        _logger.LogInformation(
            "Invoice {InvoiceId} posted via workflow {WorkflowInstanceId}",
            invoice.Id,
            notification.WorkflowInstanceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Handles workflow rejection events for Invoices
/// </summary>
public class InvoiceWorkflowRejectedEventHandler : INotificationHandler<WorkflowInstanceRejectedEvent>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvoiceWorkflowRejectedEventHandler> _logger;

    public InvoiceWorkflowRejectedEventHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ILogger<InvoiceWorkflowRejectedEventHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(WorkflowInstanceRejectedEvent notification, CancellationToken cancellationToken)
    {
        // Only handle Invoice workflows
        if (notification.EntityType != "Invoice")
            return;

        _logger.LogInformation(
            "Processing workflow rejection for Invoice {EntityId} at step {StepNumber}",
            notification.EntityId,
            notification.RejectedAtStepNumber);

        // Get the invoice
        var invoice = await _invoiceRepository.GetByIdAsync(notification.EntityId, cancellationToken);

        if (invoice == null)
        {
            _logger.LogWarning(
                "Invoice {EntityId} not found for workflow rejection",
                notification.EntityId);
            return;
        }

        // Void the invoice
        invoice.Void($"Rejected in workflow at step {notification.RejectedAtStepNumber}");

        _logger.LogInformation(
            "Invoice {InvoiceId} voided via workflow {WorkflowInstanceId}",
            invoice.Id,
            notification.WorkflowInstanceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
