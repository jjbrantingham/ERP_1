using FluentValidation;

namespace ERP.Application.BILL.Commands;

public class ApplyPaymentCommandValidator : AbstractValidator<ApplyPaymentCommand>
{
    public ApplyPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .GreaterThan(0)
            .WithMessage("Invoice ID must be greater than zero");

        RuleFor(x => x.PaymentAmount)
            .GreaterThan(0)
            .WithMessage("Payment amount must be greater than zero")
            .ScalePrecision(2, 18)
            .WithMessage("Payment amount cannot have more than 2 decimal places");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code (e.g., USD, EUR)")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be uppercase letters only");
    }
}
