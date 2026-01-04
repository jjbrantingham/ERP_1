using ERP.Domain.Common;
using ERP.Domain.FIN.Enums;

namespace ERP.Domain.FIN.Events;

public class AccountStatusChangedEvent : DomainEvent
{
    public long AccountId { get; }
    public Guid TenantId { get; }
    public AccountStatus OldStatus { get; }
    public AccountStatus NewStatus { get; }

    public AccountStatusChangedEvent(
        long accountId,
        Guid tenantId,
        AccountStatus oldStatus,
        AccountStatus newStatus)
    {
        AccountId = accountId;
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
