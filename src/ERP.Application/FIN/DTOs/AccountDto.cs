namespace ERP.Application.FIN.DTOs;

public class AccountDto
{
    public long Id { get; set; }
    public string AccountNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long? ParentAccountId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = null!;
    public bool AllowPosting { get; set; }
    public bool RequiresReconciliation { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
