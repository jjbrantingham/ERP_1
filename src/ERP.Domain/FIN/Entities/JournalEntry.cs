using ERP.Domain.Common.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Events;
using ERP.Domain.FIN.ValueObjects;

namespace ERP.Domain.FIN.Entities;

/// <summary>
/// Represents a journal entry in the general ledger
/// Implements double-entry bookkeeping: Total Debits must equal Total Credits
/// </summary>
public class JournalEntry : AggregateRoot
{
    public long Id { get; private set; }
    public Guid TenantId { get; private set; }
    public JournalEntryNumber EntryNumber { get; private set; }
    public DateTime EntryDate { get; private set; }
    public JournalEntryType Type { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    public string Description { get; private set; }
    public string? Reference { get; private set; }

    /// <summary>
    /// Fiscal period (YYYY-MM format)
    /// </summary>
    public string FiscalPeriod { get; private set; }

    private readonly List<JournalEntryLine> _lines;
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    public DateTime? PostedDate { get; private set; }
    public string? PostedBy { get; private set; }
    public DateTime? VoidedDate { get; private set; }
    public string? VoidedBy { get; private set; }
    public string? VoidReason { get; private set; }

    // Audit fields
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public byte[] RowVersion { get; private set; } = null!;

    private JournalEntry()
    {
        EntryNumber = null!;
        Description = null!;
        FiscalPeriod = null!;
        _lines = new List<JournalEntryLine>();
    }

    /// <summary>
    /// Create a new journal entry
    /// </summary>
    public static JournalEntry Create(
        Guid tenantId,
        JournalEntryNumber entryNumber,
        DateTime entryDate,
        string description,
        JournalEntryType type = JournalEntryType.General,
        string? reference = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters", nameof(description));

        var fiscalPeriod = $"{entryDate:yyyy-MM}";

        var entry = new JournalEntry
        {
            TenantId = tenantId,
            EntryNumber = entryNumber,
            EntryDate = entryDate,
            Type = type,
            Status = JournalEntryStatus.Draft,
            Description = description,
            Reference = reference,
            FiscalPeriod = fiscalPeriod,
            CreatedDate = DateTime.UtcNow
        };

        entry.AddDomainEvent(new JournalEntryCreatedEvent(entry.Id, tenantId, entryNumber.Value, entryDate));

        return entry;
    }

    /// <summary>
    /// Add a line to the journal entry
    /// </summary>
    public void AddLine(long accountId, decimal debitAmount, decimal creditAmount, string description)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Cannot modify posted or voided journal entries");

        if (debitAmount < 0 || creditAmount < 0)
            throw new ArgumentException("Debit and credit amounts must be non-negative");

        if (debitAmount > 0 && creditAmount > 0)
            throw new ArgumentException("A line cannot have both debit and credit amounts");

        if (debitAmount == 0 && creditAmount == 0)
            throw new ArgumentException("A line must have either a debit or credit amount");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Line description is required", nameof(description));

        var line = JournalEntryLine.Create(accountId, debitAmount, creditAmount, description);
        _lines.Add(line);

        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove a line from the journal entry
    /// </summary>
    public void RemoveLine(JournalEntryLine line)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Cannot modify posted or voided journal entries");

        _lines.Remove(line);
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculate total debits
    /// </summary>
    public decimal CalculateDebitTotal()
    {
        return Math.Round(_lines.Sum(x => x.DebitAmount), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Calculate total credits
    /// </summary>
    public decimal CalculateCreditTotal()
    {
        return Math.Round(_lines.Sum(x => x.CreditAmount), 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Check if the entry is balanced (debits = credits)
    /// </summary>
    public bool IsBalanced()
    {
        var totalDebits = CalculateDebitTotal();
        var totalCredits = CalculateCreditTotal();

        // Must be exact match (no tolerance for rounding errors in double-entry bookkeeping)
        return totalDebits == totalCredits;
    }

    /// <summary>
    /// Post the journal entry to the general ledger
    /// Once posted, the entry becomes immutable
    /// </summary>
    public void Post(string postedBy)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException($"Cannot post {Status} journal entries");

        if (_lines.Count == 0)
            throw new InvalidOperationException("Cannot post journal entry with no lines");

        if (!IsBalanced())
            throw new InvalidOperationException(
                $"Journal entry is not balanced. Debits ({CalculateDebitTotal():C}) must equal Credits ({CalculateCreditTotal():C})");

        if (string.IsNullOrWhiteSpace(postedBy))
            throw new ArgumentException("Posted by user is required", nameof(postedBy));

        Status = JournalEntryStatus.Posted;
        PostedDate = DateTime.UtcNow;
        PostedBy = postedBy;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new JournalEntryPostedEvent(
            Id,
            TenantId,
            EntryNumber.Value,
            CalculateDebitTotal(),
            PostedDate.Value,
            postedBy));
    }

    /// <summary>
    /// Void the journal entry
    /// Creates a reversing entry instead of deleting
    /// </summary>
    public void Void(string voidedBy, string reason)
    {
        if (Status != JournalEntryStatus.Posted)
            throw new InvalidOperationException("Can only void posted journal entries");

        if (string.IsNullOrWhiteSpace(voidedBy))
            throw new ArgumentException("Voided by user is required", nameof(voidedBy));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Void reason is required", nameof(reason));

        Status = JournalEntryStatus.Voided;
        VoidedDate = DateTime.UtcNow;
        VoidedBy = voidedBy;
        VoidReason = reason;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new JournalEntryVoidedEvent(Id, TenantId, EntryNumber.Value, reason, voidedBy));
    }

    /// <summary>
    /// Update entry details (draft only)
    /// </summary>
    public void Update(string description, string? reference = null)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Cannot modify posted or voided journal entries");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        Description = description;
        Reference = reference;
        ModifiedDate = DateTime.UtcNow;
    }
}
