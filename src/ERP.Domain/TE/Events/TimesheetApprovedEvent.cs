using ERP.Domain.Common;

namespace ERP.Domain.TE.Events;

/// <summary>
/// Domain event raised when a timesheet is approved.
/// </summary>
public class TimesheetApprovedEvent : DomainEvent
{
    public long TimesheetId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public long ApprovedByUserId { get; }
    public DateTime ApprovedAt { get; }

    public TimesheetApprovedEvent(long timesheetId, Guid tenantId, long employeeId, long approvedByUserId)
    {
        TimesheetId = timesheetId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
    }
}
