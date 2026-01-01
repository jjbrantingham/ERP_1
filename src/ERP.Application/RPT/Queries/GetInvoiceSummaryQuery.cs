using ERP.Application.RPT.DTOs;
using MediatR;

namespace ERP.Application.RPT.Queries;

public class GetInvoiceSummaryQuery : IRequest<InvoiceSummaryDto>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long? ClientId { get; set; }
    public string? Status { get; set; }
}
