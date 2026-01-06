using ERP.Domain.Common;
using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Enums;
using ERP.Domain.HR.Repositories;
using ERP.Domain.HR.ValueObjects;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Employee aggregate.
/// </summary>
public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByIdWithRatesAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Rates)
            .Include(e => e.ResourceType)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByEmployeeNumberAsync(EmployeeNumber employeeNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber, cancellationToken);
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Email.Value == email, cancellationToken);
    }

    public async Task<Employee?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => e.Status == EmployeeStatus.Active)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetAvailableEmployeesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => e.Status == EmployeeStatus.Active && e.IsAvailableForProjects)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByResourceTypeAsync(long resourceTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => e.ResourceTypeId == resourceTypeId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => e.Status == status)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => e.Department == department)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByManagerAsync(long managerId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => e.ManagerId == managerId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLowerInvariant();

        return await _context.Employees
            .Where(e =>
                e.EmployeeNumber.Value.ToLower().Contains(term) ||
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.Value.ToLower().Contains(term))
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }
}
