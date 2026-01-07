using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Validator for CreateVendorContactCommand.
/// </summary>
public class CreateVendorContactCommandValidator : AbstractValidator<CreateVendorContactCommand>
{
    public CreateVendorContactCommandValidator()
    {
        RuleFor(x => x.VendorId)
            .GreaterThan(0)
            .WithMessage("Vendor ID must be greater than zero");

        RuleFor(x => x.ContactType)
            .IsInEnum()
            .WithMessage("Invalid contact type");

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

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(BusinessConstants.Lengths.Email)
            .WithMessage($"Email must not exceed {BusinessConstants.Lengths.Email} characters");

        RuleFor(x => x.Title)
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .When(x => !string.IsNullOrEmpty(x.Title))
            .WithMessage($"Title must not exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must be in E.164 format (e.g., +12125551234)");

        RuleFor(x => x.Mobile)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.Mobile))
            .WithMessage("Mobile must be in E.164 format (e.g., +12125551234)");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage($"Notes must not exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
