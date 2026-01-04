using ERP.Domain.Common.Interfaces;
using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Enums;
using ERP.Domain.HR.ValueObjects;

namespace ERP.Domain.HR.Repositories;

/// <summary>
/// Repository interface for Employee aggregate.
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    /// <summary>
    /// Gets an employee by ID with rates included.
    /// </summary>
    Task<Employee?> GetByIdWithRatesAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an employee by employee number.
    /// </summary>
    Task<Employee?> GetByEmployeeNumberAsync(EmployeeNumber employeeNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an employee by email address.
    /// </summary>
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an employee by user ID.
    /// </summary>
    Task<Employee?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active employees.
    /// </summary>
    Task<IEnumerable<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active and available employees for project assignments.
    /// </summary>
    Task<IEnumerable<Employee>> GetAvailableEmployeesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employees by resource type.
    /// </summary>
    Task<IEnumerable<Employee>> GetByResourceTypeAsync(long resourceTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employees by status.
    /// </summary>
    Task<IEnumerable<Employee>> GetByStatusAsync(EmployeeStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employees by department.
    /// </summary>
    Task<IEnumerable<Employee>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employees by manager.
    /// </summary>
    Task<IEnumerable<Employee>> GetByManagerAsync(long managerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches employees by name or employee number.
    /// </summary>
    Task<IEnumerable<Employee>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
