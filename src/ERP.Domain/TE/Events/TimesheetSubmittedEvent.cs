using ERP.Domain.Common;

namespace ERP.Domain.TE.Events;

/// <summary>
/// Domain event raised when a timesheet is submitted for approval.
/// </summary>
public class TimesheetSubmittedEvent : DomainEvent
{
    public long TimesheetId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public DateTime SubmittedAt { get; }

    public TimesheetSubmittedEvent(long timesheetId, Guid tenantId, long employeeId)
    {
        TimesheetId = timesheetId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        SubmittedAt = DateTime.UtcNow;
    }
}
