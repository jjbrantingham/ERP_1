using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.TE.Commands;

public class CreateExpenseReportCommandValidator : AbstractValidator<CreateExpenseReportCommand>
{
    public CreateExpenseReportCommandValidator()
    {
        RuleFor(x => x.EmployeeId)
            .GreaterThan(0)
            .WithMessage("Employee ID must be greater than zero");

        RuleFor(x => x.Purpose)
            .MaximumLength(BusinessConstants.Lengths.Description)
            .WithMessage($"Purpose cannot exceed {BusinessConstants.Lengths.Description} characters");

        RuleFor(x => x.ReportDate)
            .NotEmpty()
            .When(x => x.ReportDate.HasValue)
            .WithMessage("Report date is required when provided")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(BusinessConstants.DateTime.MaxFutureDays))
            .When(x => x.ReportDate.HasValue)
            .WithMessage($"Report date cannot be more than {BusinessConstants.DateTime.MaxFutureDays} day in the future");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .WithMessage($"Notes cannot exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
