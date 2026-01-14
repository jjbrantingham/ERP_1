using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Domain.BILL.ValueObjects;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ERP.Application.BILL.Commands;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, long>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<long> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        _logger.LogInformation(
            "Creating invoice for TenantId: {TenantId}, ClientId: {ClientId}, ProjectId: {ProjectId}, BillingMode: {BillingMode}",
            _currentTenant.TenantId, request.ClientId, request.ProjectId, request.BillingMode);

        // Parse billing mode
        if (!Enum.TryParse<BillingMode>(request.BillingMode, true, out var billingMode))
        {
            _logger.LogError("Invalid billing mode provided: {BillingMode}", request.BillingMode);
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

                _logger.LogDebug("Generated invoice number: {InvoiceNumber}, Attempt: {Attempt}",
                    invoiceNumber.Value, attempt + 1);

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

                _logger.LogInformation(
                    "Successfully created invoice {InvoiceId} with number {InvoiceNumber} for client {ClientId}, Total: {TotalAmount} {Currency}",
                    invoice.Id, invoiceNumber.Value, request.ClientId, invoice.TotalAmount, request.Currency);

                return invoice.Id;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                when (ex.InnerException is SqlException sqlEx &&
                      (sqlEx.Number == 2601 || sqlEx.Number == 2627)) // Unique constraint violation
            {
                _logger.LogWarning(
                    "Invoice number collision detected on attempt {Attempt}/{MaxRetries}. Retrying...",
                    attempt + 1, maxRetries);

                if (attempt == maxRetries - 1)
                {
                    _logger.LogError(ex,
                        "Failed to generate unique invoice number after {MaxRetries} attempts for client {ClientId}",
                        maxRetries, request.ClientId);
                    throw new InvalidOperationException("Failed to generate unique invoice number after multiple attempts", ex);
                }

                // Brief delay before retry to reduce contention
                await Task.Delay(TimeSpan.FromMilliseconds(10 * (attempt + 1)), cancellationToken);
            }
        }

        _logger.LogError("Failed to create invoice for client {ClientId} - unexpected code path reached", request.ClientId);
        throw new InvalidOperationException("Failed to create invoice");
    }
}
