using MediatR;

namespace ERP.Application.FIN.Commands;

public class CreateAccountCommand : IRequest<long>
{
    public string AccountNumber { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!; // Asset, Liability, Equity, Revenue, Expense
    public string Currency { get; set; } = "USD";
    public bool AllowPosting { get; set; } = true;
    public long? ParentAccountId { get; set; }
    public bool RequiresReconciliation { get; set; } = false;
}
