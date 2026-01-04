using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.AUDIT.Commands;

/// <summary>
/// Validator for AnonymizeUserDataCommand (GDPR Right to be Forgotten).
/// </summary>
public class AnonymizeUserDataCommandValidator : AbstractValidator<AnonymizeUserDataCommand>
{
    public AnonymizeUserDataCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID must be greater than zero");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required for user data anonymization (compliance requirement)")
            .MaximumLength(BusinessConstants.Lengths.Description)
            .WithMessage($"Reason must not exceed {BusinessConstants.Lengths.Description} characters");
    }
}
