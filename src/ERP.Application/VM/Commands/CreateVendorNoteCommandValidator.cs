using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.VM.Commands;

/// <summary>
/// Validator for CreateVendorNoteCommand.
/// </summary>
public class CreateVendorNoteCommandValidator : AbstractValidator<CreateVendorNoteCommand>
{
    public CreateVendorNoteCommandValidator()
    {
        RuleFor(x => x.VendorId)
            .GreaterThan(0)
            .WithMessage("Vendor ID must be greater than zero");

        RuleFor(x => x.Subject)
            .NotEmpty()
            .WithMessage("Subject is required")
            .MaximumLength(BusinessConstants.Lengths.StandardName)
            .WithMessage($"Subject must not exceed {BusinessConstants.Lengths.StandardName} characters");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(4000)
            .WithMessage("Content must not exceed 4000 characters");

        RuleFor(x => x.NoteDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.NoteDate.HasValue)
            .WithMessage("Note date cannot be in the future");
    }
}
