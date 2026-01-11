using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.BILL.Commands;

/// <summary>
/// Validator for ApplyPaymentCommand.
/// </summary>
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
            .ScalePrecision(BusinessConstants.Currency.MoneyScale, BusinessConstants.Currency.MoneyPrecision)
            .WithMessage($"Payment amount must have at most {BusinessConstants.Currency.MoneyScale} decimal places");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .WithMessage($"Currency code must be exactly {BusinessConstants.Currency.CurrencyCodeLength} characters")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency))
            .WithMessage(x => $"Currency '{x.Currency}' is not supported. Supported currencies: {string.Join(", ", BusinessConstants.Currency.SupportedCurrencies)}");
    }
}
