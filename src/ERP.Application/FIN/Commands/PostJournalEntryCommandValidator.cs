using FluentValidation;

namespace ERP.Application.FIN.Commands;

public class PostJournalEntryCommandValidator : AbstractValidator<PostJournalEntryCommand>
{
    public PostJournalEntryCommandValidator()
    {
        RuleFor(x => x.JournalEntryId)
            .GreaterThan(0)
            .WithMessage("Journal entry ID must be greater than zero");
    }
}
