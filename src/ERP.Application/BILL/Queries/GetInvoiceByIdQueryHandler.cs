using ERP.Application.Common.Exceptions;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.BILL.DTOs;
using ERP.Domain.BILL.Repositories;
using MediatR;

namespace ERP.Application.BILL.Queries;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _invoiceRepository = invoiceRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice == null)
        {
            throw new NotFoundException($"Invoice with ID {request.InvoiceId} not found");
        }

        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber.Value,
            ClientId = invoice.ClientId,
            ProjectId = invoice.ProjectId,
            BillingMode = invoice.BillingMode.ToString(),
            Status = invoice.Status.ToString(),
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            PoNumber = invoice.PoNumber,
            Description = invoice.Description,
            TaxRate = invoice.TaxRate,
            Currency = invoice.Currency,
            Subtotal = invoice.CalculateSubtotal().Amount,
            TaxAmount = invoice.CalculateTax().Amount,
            TotalAmount = invoice.CalculateTotal().Amount,
            AmountPaid = invoice.AmountPaid.Amount,
            AmountDue = invoice.CalculateAmountDue().Amount,
            LineItems = invoice.LineItems.Select(li => new InvoiceLineItemDto
            {
                Id = li.Id,
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice.Amount,
                DiscountPercent = li.DiscountPercent,
                DiscountAmount = li.CalculateDiscountAmount(),
                LineTotal = li.CalculateLineTotal()
            }).ToList(),
            SentDate = invoice.SentDate,
            PostedDate = invoice.PostedDate,
            PaidDate = invoice.PaidDate,
            CreatedDate = invoice.CreatedDate,
            ModifiedDate = invoice.ModifiedDate
        };
    }
}
