using ERP.Domain.Common;

namespace ERP.Domain.FIN.Events;

/// <summary>
/// Domain event raised when a journal entry is reversed (creating a reversing entry).
/// </summary>
public class JournalEntryReversedEvent : DomainEvent
{
    public long JournalEntryId { get; }
    public long ReversalJournalEntryId { get; }
    public Guid TenantId { get; }
    public string EntryNumber { get; }
    public string ReversalEntryNumber { get; }
    public string Reason { get; }
    public DateTime ReversedAt { get; }

    public JournalEntryReversedEvent(
        long journalEntryId,
        long reversalJournalEntryId,
        Guid tenantId,
        string entryNumber,
        string reversalEntryNumber,
        string reason)
    {
        JournalEntryId = journalEntryId;
        ReversalJournalEntryId = reversalJournalEntryId;
        TenantId = tenantId;
        EntryNumber = entryNumber;
        ReversalEntryNumber = reversalEntryNumber;
        Reason = reason;
        ReversedAt = DateTime.UtcNow;
    }
}
