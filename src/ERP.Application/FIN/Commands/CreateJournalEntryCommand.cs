using MediatR;

namespace ERP.Application.FIN.Commands;

public class CreateJournalEntryCommand : IRequest<long>
{
    public DateTime EntryDate { get; init; }
    public string Description { get; init; } = null!;
    public string Type { get; init; } = "General"; // General, Adjusting, Closing, Reversing
    public string? Reference { get; init; }
    public List<JournalEntryLineCommand> Lines { get; init; } = new();
}

public class JournalEntryLineCommand
{
    public long AccountId { get; init; }
    public decimal DebitAmount { get; init; }
    public decimal CreditAmount { get; init; }
    public string Description { get; init; } = null!;
}
