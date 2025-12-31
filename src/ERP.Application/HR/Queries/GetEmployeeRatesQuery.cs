using ERP.Application.Common.Interfaces;
using ERP.Application.HR.DTOs;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Query to get all rates for an employee.
/// </summary>
public class GetEmployeeRatesQuery : IQuery<IEnumerable<RateDto>>
{
    public long EmployeeId { get; set; }
}
