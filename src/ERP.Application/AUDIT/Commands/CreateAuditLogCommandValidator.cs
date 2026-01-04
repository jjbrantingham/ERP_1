using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.AUDIT.Commands;

/// <summary>
/// Validator for CreateAuditLogCommand.
/// </summary>
public class CreateAuditLogCommandValidator : AbstractValidator<CreateAuditLogCommand>
{
    public CreateAuditLogCommandValidator()
    {
        RuleFor(x => x.EventType)
            .IsInEnum()
            .WithMessage("Invalid audit event type");

        RuleFor(x => x.Severity)
            .IsInEnum()
            .WithMessage("Invalid audit severity");

        RuleFor(x => x.EntityType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.EntityType))
            .WithMessage("Entity type must not exceed 100 characters");

        RuleFor(x => x.EntityId)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.EntityId))
            .WithMessage("Entity ID must not exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Audit log description is required")
            .MaximumLength(BusinessConstants.Lengths.Description)
            .WithMessage($"Description must not exceed {BusinessConstants.Lengths.Description} characters");

        RuleFor(x => x.IpAddress)
            .MaximumLength(45) // IPv6 max length
            .When(x => !string.IsNullOrEmpty(x.IpAddress))
            .WithMessage("IP address must not exceed 45 characters")
            .Matches(@"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$")
            .When(x => !string.IsNullOrEmpty(x.IpAddress))
            .WithMessage("IP address must be a valid IPv4 or IPv6 address");

        RuleFor(x => x.UserAgent)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.UserAgent))
            .WithMessage("User agent must not exceed 500 characters");

        RuleFor(x => x.Metadata)
            .MaximumLength(4000)
            .When(x => !string.IsNullOrEmpty(x.Metadata))
            .WithMessage("Metadata must not exceed 4000 characters");
    }
}
