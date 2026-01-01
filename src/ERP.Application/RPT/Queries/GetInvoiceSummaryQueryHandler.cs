using ERP.Application.RPT.DTOs;
using ERP.Domain.BILL.Repositories;
using MediatR;

namespace ERP.Application.RPT.Queries;

public class GetInvoiceSummaryQueryHandler : IRequestHandler<GetInvoiceSummaryQuery, InvoiceSummaryDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceSummaryQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceSummaryDto> Handle(GetInvoiceSummaryQuery request, CancellationToken cancellationToken)
    {
        // Get all invoices
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);

        // Apply filters
        invoices = invoices.Where(i => i.InvoiceDate >= request.StartDate && i.InvoiceDate <= request.EndDate);

        if (request.ClientId.HasValue)
            invoices = invoices.Where(i => i.ClientId == request.ClientId.Value);

        var invoiceList = invoices.ToList();
        var now = DateTime.UtcNow;

        var result = new InvoiceSummaryDto
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Invoices = invoiceList.Select(i => new InvoiceSummaryLineDto
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber.Value,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                ClientId = i.ClientId,
                ClientName = $"Client {i.ClientId}", // Would need client lookup
                Status = i.Status.ToString(),
                TotalAmount = i.CalculateTotal().Amount,
                AmountPaid = i.AmountPaid.Amount,
                AmountDue = i.CalculateAmountDue().Amount,
                DaysOutstanding = (int)(now - i.InvoiceDate).TotalDays,
                IsOverdue = i.DueDate < now && i.CalculateAmountDue().Amount > 0
            }).ToList(),
            Totals = new InvoiceSummaryTotalsDto
            {
                TotalInvoices = invoiceList.Count,
                TotalAmount = invoiceList.Sum(i => i.CalculateTotal().Amount),
                TotalPaid = invoiceList.Sum(i => i.AmountPaid.Amount),
                TotalDue = invoiceList.Sum(i => i.CalculateAmountDue().Amount),
                OverdueCount = invoiceList.Count(i => i.DueDate < now && i.CalculateAmountDue().Amount > 0),
                OverdueAmount = invoiceList
                    .Where(i => i.DueDate < now && i.CalculateAmountDue().Amount > 0)
                    .Sum(i => i.CalculateAmountDue().Amount)
            }
        };

        return result;
    }
}
