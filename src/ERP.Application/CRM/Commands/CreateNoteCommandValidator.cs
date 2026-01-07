using ERP.Shared.Constants;
using FluentValidation;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Validator for CreateNoteCommand.
/// </summary>
public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .GreaterThan(0)
            .WithMessage("Client ID must be greater than zero");

        RuleFor(x => x.ContactId)
            .GreaterThan(0)
            .When(x => x.ContactId.HasValue)
            .WithMessage("Contact ID must be greater than zero if specified");

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

        RuleFor(x => x.NoteType)
            .IsInEnum()
            .WithMessage("Invalid note type");

        RuleFor(x => x.FollowUpDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.FollowUpDate.HasValue)
            .WithMessage("Follow-up date must be in the future");

        RuleFor(x => x.AuthorId)
            .GreaterThan(0)
            .When(x => x.AuthorId.HasValue)
            .WithMessage("Author ID must be greater than zero if specified");
    }
}
