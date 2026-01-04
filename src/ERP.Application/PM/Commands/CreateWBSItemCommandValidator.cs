using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Validator for CreateWBSItemCommand.
/// </summary>
public class CreateWBSItemCommandValidator : AbstractValidator<CreateWBSItemCommand>
{
    public CreateWBSItemCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than zero");

        RuleFor(x => x.ParentId)
            .GreaterThan(0)
            .When(x => x.ParentId.HasValue)
            .WithMessage("Parent ID must be greater than zero if specified");

        RuleFor(x => x.WBSCode)
            .NotEmpty()
            .WithMessage("WBS code is required")
            .MaximumLength(50)
            .WithMessage("WBS code must not exceed 50 characters")
            .Matches(@"^[A-Z0-9]+(\.[A-Z0-9]+)*$")
            .WithMessage("WBS code must be in hierarchical format (e.g., 1.1, 1.1.1, etc.)");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("WBS item name is required")
            .MaximumLength(BusinessConstants.Lengths.Name)
            .WithMessage($"WBS item name must not exceed {BusinessConstants.Lengths.Name} characters");

        RuleFor(x => x.Description)
            .MaximumLength(BusinessConstants.Lengths.Description)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage($"Description must not exceed {BusinessConstants.Lengths.Description} characters");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sort order must be zero or greater");

        RuleFor(x => x.EstimatedHours)
            .GreaterThan(0)
            .When(x => x.EstimatedHours.HasValue)
            .WithMessage("Estimated hours must be greater than zero if specified")
            .LessThanOrEqualTo(100000)
            .When(x => x.EstimatedHours.HasValue)
            .WithMessage("Estimated hours must not exceed 100,000");

        RuleFor(x => x.BudgetAmount)
            .GreaterThan(0)
            .When(x => x.BudgetAmount.HasValue)
            .WithMessage("Budget amount must be greater than zero if specified")
            .ScalePrecision(BusinessConstants.Currency.DefaultDecimalPlaces, BusinessConstants.Currency.DefaultPrecision)
            .When(x => x.BudgetAmount.HasValue)
            .WithMessage($"Budget amount must have at most {BusinessConstants.Currency.DefaultDecimalPlaces} decimal places");

        RuleFor(x => x.BudgetCurrency)
            .NotEmpty()
            .When(x => x.BudgetAmount.HasValue)
            .WithMessage("Budget currency is required when budget amount is specified")
            .Length(BusinessConstants.Currency.CurrencyCodeLength)
            .When(x => !string.IsNullOrEmpty(x.BudgetCurrency))
            .WithMessage($"Currency code must be exactly {BusinessConstants.Currency.CurrencyCodeLength} characters")
            .Must(currency => BusinessConstants.Currency.SupportedCurrencies.Contains(currency))
            .When(x => !string.IsNullOrEmpty(x.BudgetCurrency))
            .WithMessage(x => $"Currency '{x.BudgetCurrency}' is not supported. Supported currencies: {string.Join(", ", BusinessConstants.Currency.SupportedCurrencies)}");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate.GetValueOrDefault())
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after start date");
    }
}
