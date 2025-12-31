using ERP.Domain.Common;
using ERP.Domain.CRM.Enums;

namespace ERP.Domain.CRM.Events;

/// <summary>
/// Domain event raised when a client's status changes.
/// </summary>
public class ClientStatusChangedEvent : DomainEvent
{
    public long ClientId { get; }
    public string ClientNumber { get; }
    public string ClientName { get; }
    public ClientStatus OldStatus { get; }
    public ClientStatus NewStatus { get; }
    public DateTime ChangedAt { get; }

    public ClientStatusChangedEvent(
        long clientId,
        string clientNumber,
        string clientName,
        ClientStatus oldStatus,
        ClientStatus newStatus)
    {
        ClientId = clientId;
        ClientNumber = clientNumber;
        ClientName = clientName;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = DateTime.UtcNow;
    }
}
