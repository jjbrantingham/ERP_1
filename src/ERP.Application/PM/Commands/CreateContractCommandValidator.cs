using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Validator for CreateContractCommand.
/// </summary>
public class CreateContractCommandValidator : AbstractValidator<CreateContractCommand>
{
    public CreateContractCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than zero");

        RuleFor(x => x.ContractNumber)
            .NotEmpty()
            .WithMessage("Contract number is required")
            .MaximumLength(BusinessConstants.Lengths.ReferenceNumber)
            .WithMessage($"Contract number must not exceed {BusinessConstants.Lengths.ReferenceNumber} characters");

        RuleFor(x => x.ContractType)
            .IsInEnum()
            .WithMessage("Invalid contract type");

        RuleFor(x => x.Title)
            .MaximumLength(BusinessConstants.Lengths.Name)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage($"Title must not exceed {BusinessConstants.Lengths.Name} characters");

        RuleFor(x => x.Description)
            .MaximumLength(BusinessConstants.Lengths.Description)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage($"Description must not exceed {BusinessConstants.Lengths.Description} characters");

        RuleFor(x => x.ContractValueAmount)
            .GreaterThan(0)
            .When(x => x.ContractValueAmount.HasValue)
            .WithMessage("Contract value amount must be greater than zero if specified")
            .ScalePrecision(BusinessConstants.Currency.DefaultDecimalPlaces, BusinessConstants.Currency.DefaultPrecision)
            .When(x => x.ContractValueAmount.HasValue)
            .WithMessage($"Contract value amount must have at most {BusinessConstants.Currency.DefaultDecimalPlaces} decimal places");

        RuleFor(x => x.ContractValueCurrency)
            .NotEmpty()
            .When(x => x.ContractValueAmount.HasValue)
            .WithMessage("Contract value currency is required when contract value amount is specified")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .When(x => !string.IsNullOrEmpty(x.ContractValueCurrency))
            .WithMessage($"Currency code must be exactly {BusinessConstants.Currency.CurrencyCodeLength} characters")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency))
            .When(x => !string.IsNullOrEmpty(x.ContractValueCurrency))
            .WithMessage(x => $"Currency '{x.ContractValueCurrency}' is not supported. Supported currencies: {string.Join(", ", BusinessConstants.Currency.SupportedCurrencies)}");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.SignedDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.SignedDate.HasValue)
            .WithMessage("Signed date cannot be in the future");

        RuleFor(x => x.Terms)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Terms))
            .WithMessage("Terms must not exceed 4000 characters");
    }
}
