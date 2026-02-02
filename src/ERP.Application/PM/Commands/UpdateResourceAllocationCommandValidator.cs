using FluentValidation;

namespace ERP.Application.PM.Commands;

/// <summary>
/// Validator for UpdateResourceAllocationCommand.
/// </summary>
public class UpdateResourceAllocationCommandValidator : AbstractValidator<UpdateResourceAllocationCommand>
{
    public UpdateResourceAllocationCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Resource allocation ID must be greater than 0");

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
