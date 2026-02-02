using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for ResourceAllocation entity.
/// </summary>
public class ResourceAllocationRepository : Repository<ResourceAllocation>, IResourceAllocationRepository
{
    public ResourceAllocationRepository(ERPDbContext context) : base(context)
    {
    }

    public override async Task<ResourceAllocation?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .FirstOrDefaultAsync(ra => ra.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ResourceAllocation>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .Where(ra => ra.ProjectId == projectId)
            .OrderBy(ra => ra.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResourceAllocation>> GetByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .Where(ra => ra.EmployeeId == employeeId)
            .OrderByDescending(ra => ra.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResourceAllocation>> GetActiveByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .Where(ra => ra.ProjectId == projectId && ra.IsActive)
            .OrderBy(ra => ra.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ResourceAllocation>> GetActiveByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .Where(ra => ra.EmployeeId == employeeId && ra.IsActive)
            .OrderByDescending(ra => ra.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<ResourceAllocation?> GetByProjectAndEmployeeAsync(long projectId, long employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .FirstOrDefaultAsync(ra => ra.ProjectId == projectId && ra.EmployeeId == employeeId && ra.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsAsync(long projectId, long employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .AnyAsync(ra => ra.ProjectId == projectId && ra.EmployeeId == employeeId && ra.IsActive, cancellationToken);
    }

    public async Task<decimal> GetTotalAllocatedHoursForEmployeeAsync(long employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.ResourceAllocations
            .Where(ra => ra.EmployeeId == employeeId && ra.IsActive)
            .SumAsync(ra => ra.AllocatedHoursPerWeek, cancellationToken);
    }
}
