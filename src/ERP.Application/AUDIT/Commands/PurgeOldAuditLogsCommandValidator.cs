using FluentValidation;

namespace ERP.Application.AUDIT.Commands;

/// <summary>
/// Validator for PurgeOldAuditLogsCommand.
/// </summary>
public class PurgeOldAuditLogsCommandValidator : AbstractValidator<PurgeOldAuditLogsCommand>
{
    public PurgeOldAuditLogsCommandValidator()
    {
        RuleFor(x => x.OlderThan)
            .NotEmpty()
            .WithMessage("OlderThan date is required")
            .LessThan(DateTime.UtcNow)
            .WithMessage("OlderThan date must be in the past")
            .GreaterThan(DateTime.UtcNow.AddYears(-10))
            .WithMessage("Cannot purge logs older than 10 years (safety check)");
    }
}
