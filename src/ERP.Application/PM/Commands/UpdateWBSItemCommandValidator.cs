using FluentValidation;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Validator for UpdateWBSItemCommand.
/// </summary>
public class UpdateWBSItemCommandValidator : AbstractValidator<UpdateWBSItemCommand>
{
    public UpdateWBSItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("WBS Item ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 4000 characters");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Sort order must be non-negative");

        RuleFor(x => x.EstimatedHours)
            .GreaterThanOrEqualTo(0)
            .When(x => x.EstimatedHours.HasValue)
            .WithMessage("Estimated hours must be non-negative");

        RuleFor(x => x.BudgetAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.BudgetAmount.HasValue)
            .WithMessage("Budget must be non-negative");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after start date");
    }
}
