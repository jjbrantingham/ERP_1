using ERP.Domain.Common;
using ERP.Domain.HR.ValueObjects;

namespace ERP.Domain.HR.Events;

/// <summary>
/// Domain event raised when a new employee is created.
/// </summary>
public class EmployeeCreatedEvent : DomainEvent
{
    public long EmployeeId { get; }
    public EmployeeNumber EmployeeNumber { get; }
    public string FullName { get; }
    public DateTime CreatedAt { get; }

    public EmployeeCreatedEvent(long employeeId, EmployeeNumber employeeNumber, string fullName)
    {
        EmployeeId = employeeId;
        EmployeeNumber = employeeNumber;
        FullName = fullName;
        CreatedAt = DateTime.UtcNow;
    }
}
