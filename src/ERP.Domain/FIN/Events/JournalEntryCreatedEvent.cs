using ERP.Domain.Common.Events;

namespace ERP.Domain.FIN.Events;

public class JournalEntryCreatedEvent : DomainEvent
{
    public long JournalEntryId { get; }
    public Guid TenantId { get; }
    public string EntryNumber { get; }
    public DateTime EntryDate { get; }

    public JournalEntryCreatedEvent(
        long journalEntryId,
        Guid tenantId,
        string entryNumber,
        DateTime entryDate)
    {
        JournalEntryId = journalEntryId;
        TenantId = tenantId;
        EntryNumber = entryNumber;
        EntryDate = entryDate;
    }
}
