using MediatR;

namespace ERP.Application.BILL.Commands;

public class CreateInvoiceCommand : IRequest<long>
{
    public long ClientId { get; set; }
    public long? ProjectId { get; set; }
    public string BillingMode { get; set; } = "TimeAndMaterials";
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? PoNumber { get; set; }
    public string? Description { get; set; }
    public decimal TaxRate { get; set; } = 0m;
    public string Currency { get; set; } = "USD";
    public List<InvoiceLineItemCommand> LineItems { get; set; } = new();
}

public class InvoiceLineItemCommand
{
    public string Description { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0m;
}
