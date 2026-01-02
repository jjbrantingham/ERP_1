using MediatR;

namespace ERP.Application.BILL.Commands;

public class PostInvoiceCommand : IRequest
{
    public long InvoiceId { get; init; }
}
