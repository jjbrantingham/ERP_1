using ERP.Domain.Common;

namespace ERP.Domain.PM.Entities;

/// <summary>
/// Represents a resource (employee) allocation to a project.
/// Treated as an aggregate root to support independent repository operations.
/// </summary>
public class ResourceAllocation : AggregateRoot
{
    public long ProjectId { get; private set; }
    public long EmployeeId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public decimal AllocatedHoursPerWeek { get; private set; }
    public string? Role { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    private ResourceAllocation()
    {
        // EF Core constructor
    }

    public static ResourceAllocation Create(
        Guid tenantId,
        long projectId,
        long employeeId,
        DateTime startDate,
        decimal allocatedHoursPerWeek,
        string? role = null,
        DateTime? endDate = null,
        string? notes = null)
    {
        if (allocatedHoursPerWeek <= 0)
            throw new ArgumentException("Allocated hours per week must be greater than zero", nameof(allocatedHoursPerWeek));

        if (allocatedHoursPerWeek > 168)
            throw new ArgumentException("Allocated hours per week cannot exceed 168 (hours in a week)", nameof(allocatedHoursPerWeek));

        if (endDate.HasValue && endDate.Value < startDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));

        return new ResourceAllocation
        {
            TenantId = tenantId,
            ProjectId = projectId,
            EmployeeId = employeeId,
            StartDate = startDate,
            EndDate = endDate,
            AllocatedHoursPerWeek = allocatedHoursPerWeek,
            Role = role,
            Notes = notes,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void UpdateAllocation(decimal allocatedHoursPerWeek, string? role = null)
    {
        if (allocatedHoursPerWeek <= 0)
            throw new ArgumentException("Allocated hours per week must be greater than zero", nameof(allocatedHoursPerWeek));

        if (allocatedHoursPerWeek > 168)
            throw new ArgumentException("Allocated hours per week cannot exceed 168", nameof(allocatedHoursPerWeek));

        AllocatedHoursPerWeek = allocatedHoursPerWeek;
        if (role != null)
            Role = role;

        ModifiedDate = DateTime.UtcNow;
    }

    public void End(DateTime endDate)
    {
        if (endDate < StartDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));

        EndDate = endDate;
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        EndDate = null;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }
}
