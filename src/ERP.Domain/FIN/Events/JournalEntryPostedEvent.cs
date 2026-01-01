using ERP.Domain.Common.Events;

namespace ERP.Domain.FIN.Events;

public class JournalEntryPostedEvent : DomainEvent
{
    public long JournalEntryId { get; }
    public Guid TenantId { get; }
    public string EntryNumber { get; }
    public decimal TotalAmount { get; }
    public DateTime PostedDate { get; }
    public string PostedBy { get; }

    public JournalEntryPostedEvent(
        long journalEntryId,
        Guid tenantId,
        string entryNumber,
        decimal totalAmount,
        DateTime postedDate,
        string postedBy)
    {
        JournalEntryId = journalEntryId;
        TenantId = tenantId;
        EntryNumber = entryNumber;
        TotalAmount = totalAmount;
        PostedDate = postedDate;
        PostedBy = postedBy;
    }
}
