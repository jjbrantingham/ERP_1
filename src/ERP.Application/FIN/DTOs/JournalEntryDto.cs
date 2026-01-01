namespace ERP.Application.FIN.DTOs;

public class JournalEntryDto
{
    public long Id { get; set; }
    public string EntryNumber { get; set; } = null!;
    public DateTime EntryDate { get; set; }
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Reference { get; set; }
    public string FiscalPeriod { get; set; } = null!;
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public bool IsBalanced { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new();
    public DateTime? PostedDate { get; set; }
    public string? PostedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class JournalEntryLineDto
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public string AccountNumber { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string Description { get; set; } = null!;
}
