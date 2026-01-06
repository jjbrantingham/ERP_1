using ERP.Application.RPT.DTOs;
using ERP.Domain.RPT.Enums;
using MediatR;

namespace ERP.Application.RPT.Queries;

/// <summary>
/// Query to get Project Status report
/// </summary>
public class GetProjectStatusQuery : IRequest<List<ProjectStatusDto>>
{
    public long? ProjectId { get; set; }
    public string? Status { get; set; }
    public long? ClientId { get; set; }
}

/// <summary>
/// Query to get Resource Utilization report
/// </summary>
public class GetResourceUtilizationQuery : IRequest<ResourceUtilizationDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReportPeriod Period { get; set; } = ReportPeriod.ThisMonth;
    public long? EmployeeId { get; set; }
    public string? Department { get; set; }
}

/// <summary>
/// Query to get Budget Variance report
/// </summary>
public class GetBudgetVarianceQuery : IRequest<BudgetVarianceDto>
{
    public long ProjectId { get; set; }
}
