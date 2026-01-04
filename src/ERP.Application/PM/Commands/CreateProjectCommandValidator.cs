using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.PM.Commands;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0)
            .WithMessage("Client ID must be greater than zero");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .WithMessage($"Project name cannot exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.Description)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .WithMessage($"Description cannot exceed {BusinessConstants.Lengths.LongDescription} characters");

        RuleFor(x => x.ProjectType)
            .IsInEnum()
            .WithMessage("Project type must be a valid value");

        RuleFor(x => x.BillingMode)
            .IsInEnum()
            .WithMessage("Billing mode must be a valid value");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.BudgetAmount)
            .GreaterThan(0)
            .When(x => x.BudgetAmount.HasValue)
            .WithMessage("Budget amount must be greater than zero");

        RuleFor(x => x.BudgetCurrency)
            .NotEmpty()
            .When(x => x.BudgetAmount.HasValue)
            .WithMessage("Budget currency is required when budget amount is specified")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .When(x => !string.IsNullOrWhiteSpace(x.BudgetCurrency))
            .WithMessage($"Budget currency must be a {BusinessConstants.Currency.CurrencyCodeLength}-letter ISO code")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency!))
            .When(x => !string.IsNullOrWhiteSpace(x.BudgetCurrency))
            .WithMessage("Budget currency must be a supported currency code");

        RuleFor(x => x.ProjectManagerId)
            .GreaterThan(0)
            .When(x => x.ProjectManagerId.HasValue)
            .WithMessage("Project manager ID must be greater than zero when provided");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .WithMessage($"Notes cannot exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
