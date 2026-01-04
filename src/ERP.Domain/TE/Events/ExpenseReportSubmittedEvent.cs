using ERP.Domain.Common;

namespace ERP.Domain.TE.Events;

/// <summary>
/// Domain event raised when an expense report is submitted for approval.
/// </summary>
public class ExpenseReportSubmittedEvent : DomainEvent
{
    public long ExpenseReportId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public DateTime SubmittedAt { get; }

    public ExpenseReportSubmittedEvent(long expenseReportId, Guid tenantId, long employeeId)
    {
        ExpenseReportId = expenseReportId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        SubmittedAt = DateTime.UtcNow;
    }
}
