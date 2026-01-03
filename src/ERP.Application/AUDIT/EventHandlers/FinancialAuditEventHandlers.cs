using ERP.Application.Common.Interfaces;
using ERP.Domain.AUDIT.Entities;
using ERP.Domain.AUDIT.Enums;
using ERP.Domain.AUDIT.Repositories;
using ERP.Domain.BILL.Events;
using ERP.Domain.Common;
using ERP.Domain.FIN.Events;
using ERP.Domain.FIN.Repositories;
using ERP.Domain.BILL.Repositories;
using MediatR;

namespace ERP.Application.AUDIT.EventHandlers;

/// <summary>
/// Event handler for journal entry posting - creates financial audit log.
/// </summary>
public class JournalEntryPostedAuditEventHandler : INotificationHandler<JournalEntryPostedEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public JournalEntryPostedAuditEventHandler(
        IAuditLogRepository auditLogRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _auditLogRepository = auditLogRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(JournalEntryPostedEvent notification, CancellationToken cancellationToken)
    {
        var journalEntry = await _journalEntryRepository.GetByIdAsync(notification.JournalEntryId, cancellationToken);

        if (journalEntry == null)
            return;

        var totalDebit = journalEntry.Lines.Sum(l => l.DebitAmount);

        var auditLog = AuditLog.CreateFinancialAudit(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.FinancialPost,
            entityType: "JournalEntry",
            entityId: notification.JournalEntryId,
            amount: totalDebit,
            currency: "USD", // TODO: Get from journal entry or tenant settings
            userId: _currentUserService.UserId,
            username: _currentUserService.Username ?? "System",
            description: $"Journal Entry '{journalEntry.EntryNumber}' posted to General Ledger. Description: {journalEntry.Description}",
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                EntryNumber = journalEntry.EntryNumber,
                EntryDate = journalEntry.EntryDate,
                TotalDebit = totalDebit,
                LineCount = journalEntry.Lines.Count
            })
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Event handler for journal entry reversal - creates financial audit log.
/// </summary>
public class JournalEntryReversedAuditEventHandler : INotificationHandler<JournalEntryReversedEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public JournalEntryReversedAuditEventHandler(
        IAuditLogRepository auditLogRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _auditLogRepository = auditLogRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(JournalEntryReversedEvent notification, CancellationToken cancellationToken)
    {
        var journalEntry = await _journalEntryRepository.GetByIdAsync(notification.JournalEntryId, cancellationToken);

        if (journalEntry == null)
            return;

        var totalDebit = journalEntry.Lines.Sum(l => l.DebitAmount);

        var auditLog = AuditLog.CreateFinancialAudit(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.FinancialReverse,
            entityType: "JournalEntry",
            entityId: notification.JournalEntryId,
            amount: totalDebit,
            currency: "USD",
            userId: _currentUserService.UserId,
            username: _currentUserService.Username ?? "System",
            description: $"Journal Entry '{journalEntry.EntryNumber}' reversed. Reversal Entry: {notification.ReversalEntryId}",
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                EntryNumber = journalEntry.EntryNumber,
                ReversalEntryId = notification.ReversalEntryId,
                ReversalReason = notification.Reason
            })
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Event handler for invoice posting - creates financial audit log.
/// </summary>
public class InvoicePostedAuditEventHandler : INotificationHandler<InvoicePostedEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public InvoicePostedAuditEventHandler(
        IAuditLogRepository auditLogRepository,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _auditLogRepository = auditLogRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(InvoicePostedEvent notification, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(notification.InvoiceId, cancellationToken);

        if (invoice == null)
            return;

        var auditLog = AuditLog.CreateFinancialAudit(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.InvoicePosted,
            entityType: "Invoice",
            entityId: notification.InvoiceId,
            amount: invoice.TotalAmount,
            currency: "USD",
            userId: _currentUserService.UserId,
            username: _currentUserService.Username ?? "System",
            description: $"Invoice '{invoice.InvoiceNumber}' posted. Client: {invoice.ClientId}, Amount: {invoice.TotalAmount:C}",
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                ClientId = invoice.ClientId,
                ProjectId = invoice.ProjectId,
                TotalAmount = invoice.TotalAmount,
                TaxAmount = invoice.TaxAmount
            })
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Event handler for invoice voiding - creates financial audit log.
/// </summary>
public class InvoiceVoidedAuditEventHandler : INotificationHandler<InvoiceVoidedEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public InvoiceVoidedAuditEventHandler(
        IAuditLogRepository auditLogRepository,
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _auditLogRepository = auditLogRepository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(InvoiceVoidedEvent notification, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(notification.InvoiceId, cancellationToken);

        if (invoice == null)
            return;

        var auditLog = AuditLog.CreateFinancialAudit(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.InvoiceVoided,
            entityType: "Invoice",
            entityId: notification.InvoiceId,
            amount: invoice.TotalAmount,
            currency: "USD",
            userId: _currentUserService.UserId,
            username: _currentUserService.Username ?? "System",
            description: $"Invoice '{invoice.InvoiceNumber}' voided. Reason: {notification.Reason}",
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                InvoiceNumber = invoice.InvoiceNumber,
                VoidReason = notification.Reason,
                OriginalAmount = invoice.TotalAmount
            })
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Event handler for payment received - creates financial audit log.
/// </summary>
public class PaymentReceivedAuditEventHandler : INotificationHandler<PaymentReceivedEvent>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;

    public PaymentReceivedAuditEventHandler(
        IAuditLogRepository auditLogRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService)
    {
        _auditLogRepository = auditLogRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(PaymentReceivedEvent notification, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(notification.PaymentId, cancellationToken);

        if (payment == null)
            return;

        var auditLog = AuditLog.CreateFinancialAudit(
            tenantId: _currentTenantService.TenantId,
            eventType: AuditEventType.PaymentReceived,
            entityType: "Payment",
            entityId: notification.PaymentId,
            amount: payment.Amount,
            currency: "USD",
            userId: _currentUserService.UserId,
            username: _currentUserService.Username ?? "System",
            description: $"Payment received. Amount: {payment.Amount:C}, Method: {payment.PaymentMethod}, Reference: {payment.ReferenceNumber}",
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                PaymentDate = payment.PaymentDate,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                ReferenceNumber = payment.ReferenceNumber,
                InvoiceId = payment.InvoiceId
            })
        );

        await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
