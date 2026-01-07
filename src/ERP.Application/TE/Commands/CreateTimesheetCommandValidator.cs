using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.TE.Commands;

/// <summary>
/// Validator for CreateTimesheetCommand.
/// </summary>
public class CreateTimesheetCommandValidator : AbstractValidator<CreateTimesheetCommand>
{
    public CreateTimesheetCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("Employee ID must be greater than zero");

        RuleFor(x => x.PeriodStart)
            .NotEmpty()
            .WithMessage("Period start date is required")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Period start date cannot be in the future");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty()
            .WithMessage("Period end date is required")
            .GreaterThan(x => x.PeriodStart)
            .WithMessage("Period end date must be after period start date")
            .LessThanOrEqualTo(x => x.PeriodStart.AddDays(31))
            .WithMessage("Timesheet period cannot exceed 31 days");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage($"Notes must not exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
