using ERP.Application.Common.Interfaces;
using ERP.Application.HR.DTOs;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Query to get an employee by ID.
/// </summary>
public class GetEmployeeByIdQuery : IQuery<EmployeeDto>
{
    public long EmployeeId { get; set; }
}
