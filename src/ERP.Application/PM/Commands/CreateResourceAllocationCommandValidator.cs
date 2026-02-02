using FluentValidation;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Validator for CreateResourceAllocationCommand.
/// </summary>
public class CreateResourceAllocationCommandValidator : AbstractValidator<CreateResourceAllocationCommand>
{
    public CreateResourceAllocationCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0)
            .WithMessage("Project ID must be greater than 0");

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("Employee ID must be greater than 0");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.AllocatedHoursPerWeek)
            .GreaterThan(0)
            .WithMessage("Allocated hours per week must be greater than 0")
            .LessThanOrEqualTo(168)
            .WithMessage("Allocated hours per week cannot exceed 168");

        RuleFor(x => x.Role)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Role))
            .WithMessage("Role cannot exceed 100 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes cannot exceed 4000 characters");
    }
}
