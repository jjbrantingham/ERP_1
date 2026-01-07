using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Validator for CreateContactCommand.
/// </summary>
public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    public CreateContactCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0)
            .WithMessage("Client ID must be greater than zero");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(BusinessConstants.Lengths.ShortName)
            .WithMessage($"First name must not exceed {BusinessConstants.Lengths.ShortName} characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(BusinessConstants.Lengths.ShortName)
            .WithMessage($"Last name must not exceed {BusinessConstants.Lengths.ShortName} characters");

        RuleFor(x => x.MiddleName)
            .MaximumLength(BusinessConstants.Lengths.ShortName)
            .When(x => !string.IsNullOrEmpty(x.MiddleName))
            .WithMessage($"Middle name must not exceed {BusinessConstants.Lengths.ShortName} characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(BusinessConstants.Lengths.Email)
            .WithMessage($"Email must not exceed {BusinessConstants.Lengths.Email} characters");

        RuleFor(x => x.ContactType)
            .IsInEnum()
            .WithMessage("Invalid contact type");

        RuleFor(x => x.JobTitle)
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .When(x => !string.IsNullOrEmpty(x.JobTitle))
            .WithMessage($"Job title must not exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.Department)
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .When(x => !string.IsNullOrEmpty(x.Department))
            .WithMessage($"Department must not exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number must be in E.164 format (e.g., +12125551234)");

        RuleFor(x => x.MobileNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.MobileNumber))
            .WithMessage("Mobile number must be in E.164 format (e.g., +12125551234)");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage($"Notes must not exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
