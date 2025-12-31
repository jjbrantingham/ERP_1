using ERP.Application.Common.Interfaces;
using ERP.Application.HR.DTOs;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Query to get all employees.
/// </summary>
public class GetAllEmployeesQuery : IQuery<IEnumerable<EmployeeDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
