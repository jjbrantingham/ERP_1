namespace ERP.Application.RPT.DTOs;

/// <summary>
/// Expense summary report
/// </summary>
public class ExpenseSummaryDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ExpenseSummaryLineDto> Lines { get; set; } = new();
    public decimal TotalExpenses { get; set; }
    public decimal ApprovedExpenses { get; set; }
    public decimal PendingExpenses { get; set; }
    public decimal RejectedExpenses { get; set; }
    public int EmployeeCount { get; set; }
    public int ProjectCount { get; set; }
}

/// <summary>
/// Expense summary line item
/// </summary>
public class ExpenseSummaryLineDto
{
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public long? ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

/// <summary>
/// Invoice aging report
/// </summary>
public class InvoiceAgingDto
{
    public DateTime AsOfDate { get; set; }
    public List<InvoiceAgingLineDto> Lines { get; set; } = new();
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int InvoiceCount { get; set; }
}

/// <summary>
/// Invoice aging line item
/// </summary>
public class InvoiceAgingLineDto
{
    public long InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOutstanding { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Client activity report
/// </summary>
public class ClientActivityDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ClientActivityLineDto> Lines { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
}

/// <summary>
/// Client activity line item
/// </summary>
public class ClientActivityLineDto
{
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int ProjectCount { get; set; }
    public int ActiveProjectCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingBalance { get; set; }
    public DateTime? LastInvoiceDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
}
