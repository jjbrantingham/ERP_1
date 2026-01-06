using ERP.Application.RPT.DTOs;
using MediatR;

namespace ERP.Application.RPT.Queries;

/// <summary>
/// Query to get Executive Dashboard
/// </summary>
public class GetExecutiveDashboardQuery : IRequest<ExecutiveDashboardDto>
{
    public DateTime? AsOfDate { get; set; }
}

/// <summary>
/// Query to get Project Manager Dashboard
/// </summary>
public class GetProjectManagerDashboardQuery : IRequest<ProjectManagerDashboardDto>
{
    public long ManagerId { get; set; }
    public DateTime? AsOfDate { get; set; }
}

/// <summary>
/// Query to get Finance Dashboard
/// </summary>
public class GetFinanceDashboardQuery : IRequest<FinanceDashboardDto>
{
    public DateTime? AsOfDate { get; set; }
}

/// <summary>
/// Query to get Employee Dashboard
/// </summary>
public class GetEmployeeDashboardQuery : IRequest<EmployeeDashboardDto>
{
    public long EmployeeId { get; set; }
    public DateTime? AsOfDate { get; set; }
}

/// <summary>
/// Query to get Expense Summary report
/// </summary>
public class GetExpenseSummaryQuery : IRequest<ExpenseSummaryDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long? EmployeeId { get; set; }
    public long? ProjectId { get; set; }
}
