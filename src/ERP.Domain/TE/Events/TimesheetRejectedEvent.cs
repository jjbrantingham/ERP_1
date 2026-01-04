using ERP.Domain.Common;

namespace ERP.Domain.TE.Events;

/// <summary>
/// Domain event raised when a timesheet is rejected.
/// </summary>
public class TimesheetRejectedEvent : DomainEvent
{
    public long TimesheetId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public long RejectedByUserId { get; }
    public string Reason { get; }
    public DateTime RejectedAt { get; }

    public TimesheetRejectedEvent(long timesheetId, Guid tenantId, long employeeId, long rejectedByUserId, string reason)
    {
        TimesheetId = timesheetId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        RejectedByUserId = rejectedByUserId;
        Reason = reason;
        RejectedAt = DateTime.UtcNow;
    }
}
