using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.FIN.Commands;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .WithMessage("Account number is required")
            .MaximumLength(BusinessConstants.Lengths.AccountCode)
            .WithMessage($"Account number cannot exceed {BusinessConstants.Lengths.AccountCode} characters")
            .Matches("^[0-9]{4,20}$")
            .WithMessage("Account number must contain only digits and be 4-20 characters long");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Account name is required")
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .WithMessage($"Account name cannot exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.Description)
            .MaximumLength(BusinessConstants.Lengths.Description)
            .WithMessage($"Description cannot exceed {BusinessConstants.Lengths.Description} characters");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Account type is required")
            .Must(type => new[] { "Asset", "Liability", "Equity", "Revenue", "Expense" }.Contains(type))
            .WithMessage("Account type must be one of: Asset, Liability, Equity, Revenue, Expense");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .WithMessage($"Currency must be a {BusinessConstants.Currency.CurrencyCodeLength}-letter ISO code (e.g., USD, EUR)")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency))
            .WithMessage("Currency must be a supported currency code");

        RuleFor(x => x.ParentAccountId)
            .GreaterThan(0)
            .When(x => x.ParentAccountId.HasValue)
            .WithMessage("Parent account ID must be greater than zero when provided");
    }
}
