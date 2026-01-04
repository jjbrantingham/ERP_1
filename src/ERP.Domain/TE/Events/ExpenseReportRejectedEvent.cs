using ERP.Domain.Common;

namespace ERP.Domain.TE.Events;

/// <summary>
/// Domain event raised when an expense report is rejected.
/// </summary>
public class ExpenseReportRejectedEvent : DomainEvent
{
    public long ExpenseReportId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public long RejectedByUserId { get; }
    public string Reason { get; }
    public DateTime RejectedAt { get; }

    public ExpenseReportRejectedEvent(long expenseReportId, Guid tenantId, long employeeId, long rejectedByUserId, string reason)
    {
        ExpenseReportId = expenseReportId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        RejectedByUserId = rejectedByUserId;
        Reason = reason;
        RejectedAt = DateTime.UtcNow;
    }
}
