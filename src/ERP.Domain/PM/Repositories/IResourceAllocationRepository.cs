using ERP.Domain.Common.Interfaces;
using ERP.Domain.PM.Entities;

namespace ERP.Domain.PM.Repositories;

/// <summary>
/// Repository interface for ResourceAllocation entity.
/// </summary>
public interface IResourceAllocationRepository : IRepository<ResourceAllocation>
{
    /// <summary>
    /// Get resource allocation by ID.
    /// </summary>
    new Task<ResourceAllocation?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all allocations for a project.
    /// </summary>
    Task<IEnumerable<ResourceAllocation>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all allocations for an employee.
    /// </summary>
    Task<IEnumerable<ResourceAllocation>> GetByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active allocations for a project.
    /// </summary>
    Task<IEnumerable<ResourceAllocation>> GetActiveByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active allocations for an employee.
    /// </summary>
    Task<IEnumerable<ResourceAllocation>> GetActiveByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get allocation for a specific employee on a specific project.
    /// </summary>
    Task<ResourceAllocation?> GetByProjectAndEmployeeAsync(long projectId, long employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if an employee is already allocated to a project.
    /// </summary>
    Task<bool> ExistsAsync(long projectId, long employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total allocated hours per week for an employee across all active projects.
    /// </summary>
    Task<decimal> GetTotalAllocatedHoursForEmployeeAsync(long employeeId, CancellationToken cancellationToken = default);
}
