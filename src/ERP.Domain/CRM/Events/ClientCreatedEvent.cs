using ERP.Domain.Common;

namespace ERP.Domain.CRM.Events;

/// <summary>
/// Domain event raised when a new client is created.
/// </summary>
public class ClientCreatedEvent : DomainEvent
{
    public long ClientId { get; }
    public string ClientNumber { get; }
    public string ClientName { get; }
    public DateTime CreatedAt { get; }

    public ClientCreatedEvent(long clientId, string clientNumber, string clientName)
    {
        ClientId = clientId;
        ClientNumber = clientNumber;
        ClientName = clientName;
        CreatedAt = DateTime.UtcNow;
    }
}
