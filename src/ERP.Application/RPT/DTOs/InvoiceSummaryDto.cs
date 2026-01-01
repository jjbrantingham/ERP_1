namespace ERP.Application.RPT.DTOs;

/// <summary>
/// Invoice summary report
/// </summary>
public class InvoiceSummaryDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<InvoiceSummaryLineDto> Invoices { get; set; } = new();
    public InvoiceSummaryTotalsDto Totals { get; set; } = new();
}

public class InvoiceSummaryLineDto
{
    public long InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public long ClientId { get; set; }
    public string ClientName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public int DaysOutstanding { get; set; }
    public bool IsOverdue { get; set; }
}

public class InvoiceSummaryTotalsDto
{
    public int TotalInvoices { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDue { get; set; }
    public int OverdueCount { get; set; }
    public decimal OverdueAmount { get; set; }
}
