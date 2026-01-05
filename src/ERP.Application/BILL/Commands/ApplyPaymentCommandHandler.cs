using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.BILL.Repositories;
using MediatR;

namespace ERP.Application.BILL.Commands;

public class ApplyPaymentCommandHandler : IRequestHandler<ApplyPaymentCommand>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public ApplyPaymentCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task Handle(ApplyPaymentCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        // Get invoice and verify tenant ownership
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        AuthorizationHelper.EnsureTenantOwnership(invoice, _currentTenant, "Invoice");

        // Create payment money object
        var payment = new Money(request.PaymentAmount, request.Currency);

        // Apply payment to invoice
        invoice.ApplyPayment(payment);

        // Note: PaymentAppliedEvent will be raised
        // Event handler should create journal entry in FIN module:
        // DR: Cash/Bank
        // CR: Accounts Receivable {ClientId}

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
