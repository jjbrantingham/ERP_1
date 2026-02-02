using ERP.Application.HR.DTOs;
using MediatR;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Query to get all employees.
/// </summary>
public class GetAllEmployeesQuery : IRequest<IEnumerable<EmployeeDto>>
{
    public bool ActiveOnly { get; set; } = true;
}
