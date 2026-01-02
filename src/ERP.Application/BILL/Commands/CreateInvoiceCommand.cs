using MediatR;

namespace ERP.Application.BILL.Commands;

public class CreateInvoiceCommand : IRequest<long>
{
    public long ClientId { get; init; }
    public long? ProjectId { get; init; }
    public string BillingMode { get; init; } = "TimeAndMaterials";
    public DateTime InvoiceDate { get; init; }
    public DateTime DueDate { get; init; }
    public string? PoNumber { get; init; }
    public string? Description { get; init; }
    public decimal TaxRate { get; init; } = 0m;
    public string Currency { get; init; } = "USD";
    public List<InvoiceLineItemCommand> LineItems { get; init; } = new();
}

public class InvoiceLineItemCommand
{
    public string Description { get; init; } = null!;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountPercent { get; init; } = 0m;
}
