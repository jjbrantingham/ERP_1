using ERP.Application.Common.Exceptions;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.BILL.Repositories;
using MediatR;

namespace ERP.Application.BILL.Commands;

public class ApplyPaymentCommandHandler : IRequestHandler<ApplyPaymentCommand>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplyPaymentCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ApplyPaymentCommand request, CancellationToken cancellationToken)
    {
        // Get invoice
        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new NotFoundException($"Invoice with ID {request.InvoiceId} not found");
        }

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
