using ERP.Domain.Common;
using ERP.Domain.HR.Enums;
using ERP.Domain.HR.ValueObjects;

namespace ERP.Domain.HR.Events;

/// <summary>
/// Domain event raised when an employee's status changes.
/// </summary>
public class EmployeeStatusChangedEvent : DomainEvent
{
    public long EmployeeId { get; }
    public EmployeeNumber EmployeeNumber { get; }
    public string FullName { get; }
    public EmployeeStatus OldStatus { get; }
    public EmployeeStatus NewStatus { get; }
    public string? Reason { get; }
    public DateTime ChangedAt { get; }

    public EmployeeStatusChangedEvent(
        long employeeId,
        EmployeeNumber employeeNumber,
        string fullName,
        EmployeeStatus oldStatus,
        EmployeeStatus newStatus,
        string? reason = null)
    {
        EmployeeId = employeeId;
        EmployeeNumber = employeeNumber;
        FullName = fullName;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        Reason = reason;
        ChangedAt = DateTime.UtcNow;
    }
}
