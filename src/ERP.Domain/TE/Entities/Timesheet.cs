using ERP.Domain.Common;
using ERP.Domain.TE.Enums;

namespace ERP.Domain.TE.Entities;

/// <summary>
/// Timesheet aggregate root representing an employee's time tracking for a period.
/// </summary>
public class Timesheet : AggregateRoot
{
    public long Id { get; private set; }
    public long EmployeeId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public TimesheetStatus Status { get; private set; }
    public decimal TotalHours { get; private set; }
    public DateTime? SubmittedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public long? ApprovedByUserId { get; private set; }
    public string? ApprovalComments { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<TimesheetEntry> _entries = new();
    public IReadOnlyCollection<TimesheetEntry> Entries => _entries.AsReadOnly();

    // Audit fields
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public byte[]? RowVersion { get; private set; }

    private Timesheet() { } // EF Core

    private Timesheet(
        Guid tenantId,
        long employeeId,
        DateTime periodStart,
        DateTime periodEnd)
    {
        TenantId = tenantId;
        EmployeeId = employeeId;
        PeriodStart = periodStart.Date;
        PeriodEnd = periodEnd.Date;
        Status = TimesheetStatus.Draft;
        TotalHours = 0;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new timesheet for an employee.
    /// </summary>
    public static Timesheet Create(
        Guid tenantId,
        long employeeId,
        DateTime periodStart,
        DateTime periodEnd,
        string? notes = null)
    {
        if (periodStart >= periodEnd)
            throw new ArgumentException("Period start must be before period end");

        var timesheet = new Timesheet(tenantId, employeeId, periodStart, periodEnd)
        {
            Notes = notes
        };

        return timesheet;
    }

    /// <summary>
    /// Adds a time entry to the timesheet.
    /// </summary>
    public void AddEntry(TimesheetEntry entry)
    {
        if (Status != TimesheetStatus.Draft)
            throw new InvalidOperationException("Cannot add entries to a non-draft timesheet");

        _entries.Add(entry);
        RecalculateTotalHours();
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a time entry from the timesheet.
    /// </summary>
    public void RemoveEntry(long entryId)
    {
        if (Status != TimesheetStatus.Draft)
            throw new InvalidOperationException("Cannot remove entries from a non-draft timesheet");

        var entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry != null)
        {
            _entries.Remove(entry);
            RecalculateTotalHours();
            ModifiedDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Submits the timesheet for approval.
    /// </summary>
    public void Submit()
    {
        if (Status != TimesheetStatus.Draft && Status != TimesheetStatus.Recalled)
            throw new InvalidOperationException("Can only submit draft or recalled timesheets");

        if (_entries.Count == 0)
            throw new InvalidOperationException("Cannot submit empty timesheet");

        Status = TimesheetStatus.Submitted;
        SubmittedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new TimesheetSubmittedEvent(Id, TenantId, EmployeeId));
    }

    /// <summary>
    /// Approves the timesheet.
    /// </summary>
    public void Approve(long approvedByUserId, string? comments = null)
    {
        if (Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException("Can only approve submitted timesheets");

        Status = TimesheetStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
        ApprovedByUserId = approvedByUserId;
        ApprovalComments = comments;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new TimesheetApprovedEvent(Id, TenantId, EmployeeId, approvedByUserId));
    }

    /// <summary>
    /// Rejects the timesheet.
    /// </summary>
    public void Reject(long rejectedByUserId, string reason)
    {
        if (Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException("Can only reject submitted timesheets");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));

        Status = TimesheetStatus.Rejected;
        ApprovedByUserId = rejectedByUserId;
        ApprovalComments = reason;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new TimesheetRejectedEvent(Id, TenantId, EmployeeId, rejectedByUserId, reason));
    }

    /// <summary>
    /// Recalls the timesheet back to draft status.
    /// </summary>
    public void Recall()
    {
        if (Status != TimesheetStatus.Submitted)
            throw new InvalidOperationException("Can only recall submitted timesheets");

        Status = TimesheetStatus.Recalled;
        ModifiedDate = DateTime.UtcNow;
    }

    private void RecalculateTotalHours()
    {
        TotalHours = _entries.Sum(e => e.Hours);
    }
}

/// <summary>
/// Domain event raised when timesheet is submitted.
/// </summary>
public class TimesheetSubmittedEvent : DomainEvent
{
    public long TimesheetId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }

    public TimesheetSubmittedEvent(long timesheetId, Guid tenantId, long employeeId)
    {
        TimesheetId = timesheetId;
        TenantId = tenantId;
        EmployeeId = employeeId;
    }
}

/// <summary>
/// Domain event raised when timesheet is approved.
/// </summary>
public class TimesheetApprovedEvent : DomainEvent
{
    public long TimesheetId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public long ApprovedByUserId { get; }

    public TimesheetApprovedEvent(long timesheetId, Guid tenantId, long employeeId, long approvedByUserId)
    {
        TimesheetId = timesheetId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        ApprovedByUserId = approvedByUserId;
    }
}

/// <summary>
/// Domain event raised when timesheet is rejected.
/// </summary>
public class TimesheetRejectedEvent : DomainEvent
{
    public long TimesheetId { get; }
    public Guid TenantId { get; }
    public long EmployeeId { get; }
    public long RejectedByUserId { get; }
    public string Reason { get; }

    public TimesheetRejectedEvent(long timesheetId, Guid tenantId, long employeeId, long rejectedByUserId, string reason)
    {
        TimesheetId = timesheetId;
        TenantId = tenantId;
        EmployeeId = employeeId;
        RejectedByUserId = rejectedByUserId;
        Reason = reason;
    }
}
