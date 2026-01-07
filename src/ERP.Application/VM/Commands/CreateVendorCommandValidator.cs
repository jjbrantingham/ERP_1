using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Validator for CreateVendorCommand.
/// </summary>
public class CreateVendorCommandValidator : AbstractValidator<CreateVendorCommand>
{
    public CreateVendorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Vendor name is required")
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .WithMessage($"Vendor name must not exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.VendorType)
            .IsInEnum()
            .WithMessage("Invalid vendor type");

        RuleFor(x => x.TaxId)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.TaxId))
            .WithMessage("Tax ID must not exceed 50 characters");

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Website must not exceed 200 characters")
            .Matches(@"^https?://")
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Website must start with http:// or https://");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Email must be a valid email address")
            .MaximumLength(BusinessConstants.Lengths.Email)
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage($"Email must not exceed {BusinessConstants.Lengths.Email} characters");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must be in E.164 format (e.g., +12125551234)");

        RuleFor(x => x.Notes)
            .MaximumLength(BusinessConstants.Lengths.LongDescription)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage($"Notes must not exceed {BusinessConstants.Lengths.LongDescription} characters");
    }
}
