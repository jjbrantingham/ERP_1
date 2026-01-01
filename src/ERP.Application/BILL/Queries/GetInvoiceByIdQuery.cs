using ERP.Application.BILL.DTOs;
using MediatR;

namespace ERP.Application.BILL.Queries;

public class GetInvoiceByIdQuery : IRequest<InvoiceDto>
{
    public long InvoiceId { get; set; }
}
