using ERP.Domain.Common.Events;
using ERP.Domain.FIN.Enums;

namespace ERP.Domain.FIN.Events;

public class AccountCreatedEvent : DomainEvent
{
    public long AccountId { get; }
    public Guid TenantId { get; }
    public string AccountNumber { get; }
    public AccountType Type { get; }

    public AccountCreatedEvent(long accountId, Guid tenantId, string accountNumber, AccountType type)
    {
        AccountId = accountId;
        TenantId = tenantId;
        AccountNumber = accountNumber;
        Type = type;
    }
}
