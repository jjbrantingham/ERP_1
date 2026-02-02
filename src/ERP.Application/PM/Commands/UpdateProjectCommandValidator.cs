using FluentValidation;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Validator for UpdateProjectCommand.
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required")
            .MaximumLength(200)
            .WithMessage("Project name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(4000)
            .WithMessage("Description cannot exceed 4000 characters");

        RuleFor(x => x.ProjectType)
            .IsInEnum()
            .WithMessage("Invalid project type");

        RuleFor(x => x.BillingMode)
            .IsInEnum()
            .WithMessage("Invalid billing mode");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.BudgetAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.BudgetAmount.HasValue)
            .WithMessage("Budget amount must be non-negative");
    }
}
