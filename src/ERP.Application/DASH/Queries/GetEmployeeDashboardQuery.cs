using ERP.Application.DASH.DTOs;
using MediatR;

namespace ERP.Application.DASH.Queries;

/// <summary>
/// Query to get employee dashboard data
/// </summary>
public class GetEmployeeDashboardQuery : IRequest<EmployeeDashboardDto>
{
    /// <summary>
    /// Employee ID (if not specified, uses current user's employee record)
    /// </summary>
    public long? EmployeeId { get; set; }

    /// <summary>
    /// As of date (defaults to today)
    /// </summary>
    public DateTime? AsOfDate { get; set; }
}
