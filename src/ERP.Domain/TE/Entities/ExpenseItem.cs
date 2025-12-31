using ERP.Domain.Common;
using ERP.Domain.Common.ValueObjects;
using ERP.Domain.TE.Enums;

namespace ERP.Domain.TE.Entities;

/// <summary>
/// Individual expense item within an expense report.
/// </summary>
public class ExpenseItem : Entity
{
    public long Id { get; private set; }
    public long ExpenseReportId { get; private set; }
    public long? ProjectId { get; private set; }
    public DateTime ExpenseDate { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = null!;
    public string? Merchant { get; private set; }
    public string? ReceiptNumber { get; private set; }
    public bool HasReceipt { get; private set; }
    public bool IsReimbursable { get; private set; }

    // Navigation property
    public ExpenseReport ExpenseReport { get; private set; } = null!;

    // Audit fields
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }

    private ExpenseItem() { } // EF Core

    private ExpenseItem(
        Guid tenantId,
        long expenseReportId,
        DateTime expenseDate,
        ExpenseCategory category,
        string description,
        Money amount)
    {
        TenantId = tenantId;
        ExpenseReportId = expenseReportId;
        ExpenseDate = expenseDate.Date;
        Category = category;
        Description = description;
        Amount = amount;
        IsReimbursable = true;
        HasReceipt = false;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new expense item.
    /// </summary>
    public static ExpenseItem Create(
        Guid tenantId,
        long expenseReportId,
        DateTime expenseDate,
        ExpenseCategory category,
        string description,
        Money amount,
        long? projectId = null,
        string? merchant = null,
        string? receiptNumber = null,
        bool hasReceipt = false,
        bool isReimbursable = true)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        if (amount.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        var item = new ExpenseItem(tenantId, expenseReportId, expenseDate, category, description, amount)
        {
            ProjectId = projectId,
            Merchant = merchant,
            ReceiptNumber = receiptNumber,
            HasReceipt = hasReceipt,
            IsReimbursable = isReimbursable
        };

        return item;
    }

    /// <summary>
    /// Updates the expense item details.
    /// </summary>
    public void Update(
        ExpenseCategory category,
        string description,
        Money amount,
        long? projectId = null,
        string? merchant = null,
        string? receiptNumber = null,
        bool hasReceipt = false,
        bool isReimbursable = true)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        if (amount.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        Category = category;
        Description = description;
        Amount = amount;
        ProjectId = projectId;
        Merchant = merchant;
        ReceiptNumber = receiptNumber;
        HasReceipt = hasReceipt;
        IsReimbursable = isReimbursable;
        ModifiedDate = DateTime.UtcNow;
    }
}
