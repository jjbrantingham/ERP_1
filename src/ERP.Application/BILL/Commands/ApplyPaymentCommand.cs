using ERP.Shared.Constants;
using MediatR;

namespace ERP.Application.BILL.Commands;

public class ApplyPaymentCommand : IRequest
{
    public long InvoiceId { get; init; }
    public decimal PaymentAmount { get; init; }
    public string Currency { get; init; } = BusinessConstants.Currency.DefaultCurrency;
}
