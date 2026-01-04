using ERP.Domain.Common;
using ERP.Domain.VM.Enums;

namespace ERP.Domain.VM.Events;

/// <summary>
/// Domain event raised when vendor status changes.
/// </summary>
public class VendorStatusChangedEvent : DomainEvent
{
    public long VendorId { get; }
    public Guid TenantId { get; }
    public VendorStatus OldStatus { get; }
    public VendorStatus NewStatus { get; }
    public DateTime ChangedAt { get; }

    public VendorStatusChangedEvent(
        long vendorId,
        Guid tenantId,
        VendorStatus oldStatus,
        VendorStatus newStatus)
    {
        VendorId = vendorId;
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = DateTime.UtcNow;
    }
}
