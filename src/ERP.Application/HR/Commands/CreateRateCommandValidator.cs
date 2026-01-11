using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.HR.Commands;

/// <summary>
/// Validator for CreateRateCommand.
/// </summary>
public class CreateRateCommandValidator : AbstractValidator<CreateRateCommand>
{
    public CreateRateCommandValidator()
    {
        // Must have either EmployeeId or ResourceTypeId, but not both
        RuleFor(x => x)
            .Must(x => (x.EmployeeId.HasValue && !x.ResourceTypeId.HasValue) ||
                      (!x.EmployeeId.HasValue && x.ResourceTypeId.HasValue))
            .WithMessage("Rate must be associated with either an Employee or a Resource Type, but not both");

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .When(x => x.EmployeeId.HasValue)
            .WithMessage("Employee ID must be greater than zero if specified");

        RuleFor(x => x.ResourceTypeId)
            .GreaterThan(0)
            .When(x => x.ResourceTypeId.HasValue)
            .WithMessage("Resource Type ID must be greater than zero if specified");

        RuleFor(x => x.RateType)
            .IsInEnum()
            .WithMessage("Invalid rate type");

        RuleFor(x => x.CostRateAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Cost rate amount must be zero or greater")
            .ScalePrecision(BusinessConstants.Currency.MoneyScale, BusinessConstants.Currency.MoneyPrecision)
            .WithMessage($"Cost rate amount must have at most {BusinessConstants.Currency.MoneyScale} decimal places");

        RuleFor(x => x.CostRateCurrency)
            .NotEmpty()
            .WithMessage("Cost rate currency is required")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .WithMessage($"Currency code must be exactly {BusinessConstants.Currency.CurrencyCodeLength} characters")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency))
            .WithMessage(x => $"Currency '{x.CostRateCurrency}' is not supported. Supported currencies: {string.Join(", ", BusinessConstants.Currency.SupportedCurrencies)}");

        RuleFor(x => x.BillingRateAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Billing rate amount must be zero or greater")
            .ScalePrecision(BusinessConstants.Currency.MoneyScale, BusinessConstants.Currency.MoneyPrecision)
            .WithMessage($"Billing rate amount must have at most {BusinessConstants.Currency.MoneyScale} decimal places");

        RuleFor(x => x.BillingRateCurrency)
            .NotEmpty()
            .WithMessage("Billing rate currency is required")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .WithMessage($"Currency code must be exactly {BusinessConstants.Currency.CurrencyCodeLength} characters")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency))
            .WithMessage(x => $"Currency '{x.BillingRateCurrency}' is not supported. Supported currencies: {string.Join(", ", BusinessConstants.Currency.SupportedCurrencies)}");

        RuleFor(x => x.EffectiveDate)
            .NotEmpty()
            .WithMessage("Effective date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.EffectiveDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after effective date");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage($"Notes must not exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
