using MediatR;

namespace ERP.Application.FIN.Commands;

public class CreateAccountCommand : IRequest<long>
{
    public string AccountNumber { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string Type { get; init; } = null!; // Asset, Liability, Equity, Revenue, Expense
    public string Currency { get; init; } = "USD";
    public bool AllowPosting { get; init; } = true;
    public long? ParentAccountId { get; init; }
    public bool RequiresReconciliation { get; init; } = false;
}
