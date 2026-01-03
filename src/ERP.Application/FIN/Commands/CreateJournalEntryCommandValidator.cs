using FluentValidation;

namespace ERP.Application.FIN.Commands;

public class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(x => x.EntryDate)
            .NotEmpty()
            .WithMessage("Entry date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Entry date cannot be more than 1 day in the future");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required")
            .Must(type => new[] { "General", "Adjusting", "Closing", "Reversing" }.Contains(type))
            .WithMessage("Type must be one of: General, Adjusting, Closing, Reversing");

        RuleFor(x => x.Reference)
            .MaximumLength(100)
            .WithMessage("Reference cannot exceed 100 characters");

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("At least one journal entry line is required")
            .Must(lines => lines.Count >= 2)
            .WithMessage("At least two journal entry lines are required (debit and credit)");

        // Validate double-entry bookkeeping: total debits must equal total credits
        RuleFor(x => x.Lines)
            .Must(BeBalanced)
            .WithMessage("Total debits must equal total credits");

        RuleForEach(x => x.Lines)
            .SetValidator(new JournalEntryLineCommandValidator());
    }

    private bool BeBalanced(List<JournalEntryLineCommand> lines)
    {
        if (lines == null || !lines.Any())
            return false;

        var totalDebits = lines.Sum(l => l.DebitAmount);
        var totalCredits = lines.Sum(l => l.CreditAmount);

        return totalDebits == totalCredits && totalDebits > 0;
    }
}

public class JournalEntryLineCommandValidator : AbstractValidator<JournalEntryLineCommand>
{
    public JournalEntryLineCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(0)
            .WithMessage("Account ID must be greater than zero");

        RuleFor(x => x.DebitAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Debit amount must be greater than or equal to zero");

        RuleFor(x => x.CreditAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Credit amount must be greater than or equal to zero");

        // Either debit or credit must be non-zero, but not both
        RuleFor(x => x)
            .Must(line => (line.DebitAmount > 0 && line.CreditAmount == 0) ||
                         (line.CreditAmount > 0 && line.DebitAmount == 0))
            .WithMessage("Each line must have either a debit or a credit amount, but not both");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Line description is required")
            .MaximumLength(500)
            .WithMessage("Line description cannot exceed 500 characters");
    }
}
