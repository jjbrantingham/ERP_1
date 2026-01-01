using ERP.Application.Common.Exceptions;
using ERP.Application.BILL.DTOs;
using ERP.Domain.BILL.Repositories;
using MediatR;

namespace ERP.Application.BILL.Queries;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
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
