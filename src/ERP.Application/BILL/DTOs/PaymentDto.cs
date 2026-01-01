namespace ERP.Application.BILL.DTOs;

public class PaymentDto
{
    public long Id { get; set; }
    public string PaymentNumber { get; set; } = null!;
    public long ClientId { get; set; }
    public long? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Method { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime PaymentDate { get; set; }
    public DateTime? ClearedDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
}
