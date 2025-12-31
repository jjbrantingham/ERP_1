using ERP.Domain.Common;

namespace ERP.Domain.TE.Entities;

/// <summary>
/// Individual time entry within a timesheet.
/// </summary>
public class TimesheetEntry : Entity
{
    public long Id { get; private set; }
    public long TimesheetId { get; private set; }
    public long? ProjectId { get; private set; }
    public long? WBSItemId { get; private set; }
    public DateTime WorkDate { get; private set; }
    public decimal Hours { get; private set; }
    public string? Description { get; private set; }
    public bool IsBillable { get; private set; }

    // Navigation property
    public Timesheet Timesheet { get; private set; } = null!;

    // Audit fields
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }

    private TimesheetEntry() { } // EF Core

    private TimesheetEntry(
        Guid tenantId,
        long timesheetId,
        DateTime workDate,
        decimal hours)
    {
        TenantId = tenantId;
        TimesheetId = timesheetId;
        WorkDate = workDate.Date;
        Hours = hours;
        IsBillable = false;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new timesheet entry.
    /// </summary>
    public static TimesheetEntry Create(
        Guid tenantId,
        long timesheetId,
        DateTime workDate,
        decimal hours,
        long? projectId = null,
        long? wbsItemId = null,
        string? description = null,
        bool isBillable = false)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be greater than zero", nameof(hours));

        if (hours > 24)
            throw new ArgumentException("Hours cannot exceed 24 per day", nameof(hours));

        var entry = new TimesheetEntry(tenantId, timesheetId, workDate, hours)
        {
            ProjectId = projectId,
            WBSItemId = wbsItemId,
            Description = description,
            IsBillable = isBillable
        };

        return entry;
    }

    /// <summary>
    /// Updates the entry details.
    /// </summary>
    public void Update(
        decimal hours,
        long? projectId = null,
        long? wbsItemId = null,
        string? description = null,
        bool isBillable = false)
    {
        if (hours <= 0)
            throw new ArgumentException("Hours must be greater than zero", nameof(hours));

        if (hours > 24)
            throw new ArgumentException("Hours cannot exceed 24 per day", nameof(hours));

        Hours = hours;
        ProjectId = projectId;
        WBSItemId = wbsItemId;
        Description = description;
        IsBillable = isBillable;
        ModifiedDate = DateTime.UtcNow;
    }
}
