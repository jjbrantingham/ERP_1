using ERP.Domain.Common;

namespace ERP.Domain.TE.Events;

/// <summary>
/// Domain event raised when an expense report is approved.
/// </summary>
public class ExpenseReportApprovedEvent : DomainEvent
{
    public long ExpenseReportId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public long ApprovedByUserId { get; }
    public DateTime ApprovedAt { get; }

    public ExpenseReportApprovedEvent(long expenseReportId, Guid tenantId, long employeeId, long approvedByUserId)
    {
        ExpenseReportId = expenseReportId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
    }
}
