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
        // Parse billing mode
        if (!Enum.TryParse<BillingMode>(request.BillingMode, true, out var billingMode))
        {
            throw new ArgumentException($"Invalid billing mode: {request.BillingMode}");
        }

        // Retry logic for handling unique constraint violations on invoice number
        const int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                // Generate unique invoice number
                var invoiceNumber = InvoiceNumber.Generate();

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
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
                      (sqlEx.Number == 2601 || sqlEx.Number == 2627)) // Unique constraint violation
            {
                if (attempt == maxRetries - 1)
                    throw new InvalidOperationException("Failed to generate unique invoice number after multiple attempts", ex);

                // Brief delay before retry to reduce contention
                await Task.Delay(TimeSpan.FromMilliseconds(10 * (attempt + 1)), cancellationToken);
            }
        }

        throw new InvalidOperationException("Failed to create invoice");
    }
}
