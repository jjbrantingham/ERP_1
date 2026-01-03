using FluentValidation;

namespace ERP.Application.TE.Commands;

public class CreateTimesheetCommandValidator : AbstractValidator<CreateTimesheetCommand>
{
    public CreateTimesheetCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("Employee ID must be greater than zero");

        RuleFor(x => x.PeriodStart)
            .NotEmpty()
            .WithMessage("Period start date is required");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty()
            .WithMessage("Period end date is required");

        RuleFor(x => x.PeriodEnd)
            .GreaterThan(x => x.PeriodStart)
            .WithMessage("Period end date must be after period start date");

        RuleFor(x => x.Notes)
            .MaximumLength(4000)
            .WithMessage("Notes cannot exceed 4000 characters");
    }
}
