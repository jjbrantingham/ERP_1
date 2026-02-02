using ERP.Application.HR.DTOs;
using MediatR;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Query to get an employee by ID.
/// </summary>
public class GetEmployeeByIdQuery : IRequest<EmployeeDto>
{
    public long EmployeeId { get; set; }
}
