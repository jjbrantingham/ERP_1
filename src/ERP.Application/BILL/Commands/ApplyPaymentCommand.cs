using MediatR;

namespace ERP.Application.BILL.Commands;

public class ApplyPaymentCommand : IRequest
{
    public long InvoiceId { get; set; }
    public decimal PaymentAmount { get; set; }
    public string Currency { get; set; } = "USD";
}
