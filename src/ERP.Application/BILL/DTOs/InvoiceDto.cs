namespace ERP.Application.BILL.DTOs;

public class InvoiceDto
{
    public long Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public long ClientId { get; set; }
    public long? ProjectId { get; set; }
    public string BillingMode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? PoNumber { get; set; }
    public string? Description { get; set; }
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public List<InvoiceLineItemDto> LineItems { get; set; } = new();
    public DateTime? SentDate { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class InvoiceLineItemDto
{
    public long Id { get; set; }
    public string Description { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}
