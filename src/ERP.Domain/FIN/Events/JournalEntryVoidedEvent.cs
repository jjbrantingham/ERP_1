using ERP.Domain.Common.Events;

namespace ERP.Domain.FIN.Events;

public class JournalEntryVoidedEvent : DomainEvent
{
    public long JournalEntryId { get; }
    public Guid TenantId { get; }
    public string EntryNumber { get; }
    public string Reason { get; }
    public string VoidedBy { get; }

    public JournalEntryVoidedEvent(
        long journalEntryId,
        Guid tenantId,
        string entryNumber,
        string reason,
        string voidedBy)
    {
        JournalEntryId = journalEntryId;
        TenantId = tenantId;
        EntryNumber = entryNumber;
        Reason = reason;
        VoidedBy = voidedBy;
    }
}
