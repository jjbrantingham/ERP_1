using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.TE.Enums;

namespace ERP.Domain.TE.Entities;

/// <summary>
/// Expense report aggregate root representing an employee's expense claims for a period.
/// </summary>
public class ExpenseReport : AggregateRoot
{
    public long Id { get; private set; }
    public long EmployeeId { get; private set; }
    public string ReportNumber { get; private set; } = string.Empty;
    public string? Purpose { get; private set; }
    public DateTime ReportDate { get; private set; }
    public ExpenseStatus Status { get; private set; }
    public Money TotalAmount { get; private set; } = null!;
    public DateTime? SubmittedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public long? ApprovedByUserId { get; private set; }
    public string? ApprovalComments { get; private set; }
    public DateTime? ReimbursedDate { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<ExpenseItem> _items = new();
    public IReadOnlyCollection<ExpenseItem> Items => _items.AsReadOnly();

    // Audit fields
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public byte[]? RowVersion { get; private set; }

    private ExpenseReport() { } // EF Core

    private ExpenseReport(
        Guid tenantId,
        long employeeId,
        string reportNumber,
        DateTime reportDate)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        ReportNumber = reportNumber;
        ReportDate = reportDate.Date;
        Status = ExpenseStatus.Draft;
        TotalAmount = new Money(0, "USD");
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new expense report.
    /// </summary>
    public static ExpenseReport Create(
        Guid tenantId,
        long employeeId,
        string reportNumber,
        DateTime? reportDate = null,
        string? purpose = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(reportNumber))
            throw new ArgumentException("Report number is required", nameof(reportNumber));

        var report = new ExpenseReport(
            tenantId,
            employeeId,
            reportNumber,
            reportDate ?? DateTime.UtcNow)
        {
            Purpose = purpose,
            Notes = notes
        };

        return report;
    }

    /// <summary>
    /// Adds an expense item to the report.
    /// </summary>
    public void AddItem(ExpenseItem item)
    {
        if (Status != ExpenseStatus.Draft)
            throw new InvalidOperationException("Cannot add items to a non-draft expense report");

        _items.Add(item);
        RecalculateTotalAmount();
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes an expense item from the report.
    /// </summary>
    public void RemoveItem(long itemId)
    {
        if (Status != ExpenseStatus.Draft)
            throw new InvalidOperationException("Cannot remove items from a non-draft expense report");

        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotalAmount();
            ModifiedDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Submits the expense report for approval.
    /// </summary>
    public void Submit()
    {
        if (Status != ExpenseStatus.Draft && Status != ExpenseStatus.Recalled)
            throw new InvalidOperationException("Can only submit draft or recalled expense reports");

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot submit empty expense report");

        Status = ExpenseStatus.Submitted;
        SubmittedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new ExpenseReportSubmittedEvent(Id, TenantId, EmployeeId));
    }

    /// <summary>
    /// Approves the expense report.
    /// </summary>
    public void Approve(long approvedByUserId, string? comments = null)
    {
        if (Status != ExpenseStatus.Submitted)
            throw new InvalidOperationException("Can only approve submitted expense reports");

        Status = ExpenseStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
        ApprovedByUserId = approvedByUserId;
        ApprovalComments = comments;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new ExpenseReportApprovedEvent(Id, TenantId, EmployeeId, approvedByUserId));
    }

    /// <summary>
    /// Rejects the expense report.
    /// </summary>
    public void Reject(long rejectedByUserId, string reason)
    {
        if (Status != ExpenseStatus.Submitted)
            throw new InvalidOperationException("Can only reject submitted expense reports");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));

        Status = ExpenseStatus.Rejected;
        ApprovedByUserId = rejectedByUserId;
        ApprovalComments = reason;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new ExpenseReportRejectedEvent(Id, TenantId, EmployeeId, rejectedByUserId, reason));
    }

    /// <summary>
    /// Recalls the expense report back to draft status.
    /// </summary>
    public void Recall()
    {
        if (Status != ExpenseStatus.Submitted)
            throw new InvalidOperationException("Can only recall submitted expense reports");

        Status = ExpenseStatus.Recalled;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the expense report as reimbursed.
    /// </summary>
    public void MarkAsReimbursed()
    {
        if (Status != ExpenseStatus.Approved)
            throw new InvalidOperationException("Can only reimburse approved expense reports");

        Status = ExpenseStatus.Reimbursed;
        ReimbursedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    private void RecalculateTotalAmount()
    {
        if (_items.Count > 0)
        {
            var currency = _items.First().Amount.Currency;
            var total = _items.Sum(i => i.Amount.Amount);
            TotalAmount = new Money(total, currency);
        }
        else
        {
            TotalAmount = new Money(0, "USD");
        }
    }
}

/// <summary>
/// Domain event raised when expense report is submitted.
/// </summary>
public class ExpenseReportSubmittedEvent : DomainEvent
{
    public long ExpenseReportId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }

    public ExpenseReportSubmittedEvent(long expenseReportId, Guid tenantId, long employeeId)
    {
        ExpenseReportId = expenseReportId;
        TenantId = tenantId;
        EmployeeId = employeeId;
    }
}

/// <summary>
/// Domain event raised when expense report is approved.
/// </summary>
public class ExpenseReportApprovedEvent : DomainEvent
{
    public long ExpenseReportId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public long ApprovedByUserId { get; }

    public ExpenseReportApprovedEvent(long expenseReportId, Guid tenantId, long employeeId, long approvedByUserId)
    {
        ExpenseReportId = expenseReportId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        ApprovedByUserId = approvedByUserId;
    }
}

/// <summary>
/// Domain event raised when expense report is rejected.
/// </summary>
public class ExpenseReportRejectedEvent : DomainEvent
{
    public long ExpenseReportId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public long RejectedByUserId { get; }
    public string Reason { get; }

    public ExpenseReportRejectedEvent(long expenseReportId, Guid tenantId, long employeeId, long rejectedByUserId, string reason)
    {
        ExpenseReportId = expenseReportId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        RejectedByUserId = rejectedByUserId;
        Reason = reason;
    }
}
