using MediatR;

namespace ERP.Application.FIN.Commands;

public class CreateJournalEntryCommand : IRequest<long>
{
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = null!;
    public string Type { get; set; } = "General"; // General, Adjusting, Closing, Reversing
    public string? Reference { get; set; }
    public List<JournalEntryLineCommand> Lines { get; set; } = new();
}

public class JournalEntryLineCommand
{
    public long AccountId { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Description { get; set; } = null!;
}
