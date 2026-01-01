using ERP.Application.Common.Interfaces;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Domain.BILL.ValueObjects;
using MediatR;

namespace ERP.Application.BILL.Commands;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, long>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Generate unique invoice number
        var invoiceNumber = InvoiceNumber.Generate();
        while (await _invoiceRepository.ExistsAsync(invoiceNumber, cancellationToken))
        {
            invoiceNumber = InvoiceNumber.Generate();
        }

        // Parse billing mode
        if (!Enum.TryParse<BillingMode>(request.BillingMode, true, out var billingMode))
        {
            throw new ArgumentException($"Invalid billing mode: {request.BillingMode}");
        }

        // Create invoice
        var invoice = Invoice.Create(
            _currentTenant.TenantId,
            invoiceNumber,
            request.ClientId,
            request.InvoiceDate,
            request.DueDate,
            billingMode,
            request.Currency,
            request.ProjectId,
            request.PoNumber,
            request.Description,
            request.TaxRate
        );

        // Add line items
        foreach (var lineItemCommand in request.LineItems)
        {
            var unitPrice = new Money(lineItemCommand.UnitPrice, request.Currency);

            invoice.AddLineItem(
                lineItemCommand.Description,
                lineItemCommand.Quantity,
                unitPrice,
                lineItemCommand.DiscountPercent
            );
        }

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.Id;
    }
}
