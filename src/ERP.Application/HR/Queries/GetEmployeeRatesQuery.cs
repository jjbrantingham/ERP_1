using ERP.Application.HR.DTOs;
using MediatR;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Query to get all rates for an employee.
/// </summary>
public class GetEmployeeRatesQuery : IRequest<IEnumerable<RateDto>>
{
    public long EmployeeId { get; set; }
}
