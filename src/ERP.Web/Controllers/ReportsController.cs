using ERP.Application.RPT.DTOs;
using ERP.Application.RPT.Queries;
using ERP.Domain.RPT.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("financial/profit-and-loss")]
    [Authorize(Roles = "Administrator,AccountingManager,Finance")]
    public async Task<IActionResult> GetProfitAndLoss(
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, 
        [FromQuery] ReportPeriod period = ReportPeriod.ThisMonth)
    {
        var result = await _mediator.Send(new GetProfitAndLossQuery { StartDate = startDate, EndDate = endDate, Period = period });
        return Ok(result);
    }

    [HttpGet("financial/balance-sheet")]
    [Authorize(Roles = "Administrator,AccountingManager,Finance")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime? asOfDate)
    {
        var result = await _mediator.Send(new GetBalanceSheetQuery { AsOfDate = asOfDate ?? DateTime.UtcNow });
        return Ok(result);
    }

    [HttpGet("financial/cash-flow")]
    [Authorize(Roles = "Administrator,AccountingManager,Finance")]
    public async Task<IActionResult> GetCashFlow([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] ReportPeriod period = ReportPeriod.ThisMonth)
    {
        var result = await _mediator.Send(new GetCashFlowQuery { StartDate = startDate, EndDate = endDate, Period = period });
        return Ok(result);
    }

    [HttpGet("financial/ar-aging")]
    [Authorize(Roles = "Administrator,AccountingManager,Finance")]
    public async Task<IActionResult> GetARAgingReport([FromQuery] DateTime? asOfDate)
    {
        var result = await _mediator.Send(new GetARAgingQuery { AsOfDate = asOfDate });
        return Ok(result);
    }

    [HttpGet("projects/status")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> GetProjectStatus([FromQuery] long? projectId, [FromQuery] string? status, [FromQuery] long? clientId)
    {
        var result = await _mediator.Send(new GetProjectStatusQuery { ProjectId = projectId, Status = status, ClientId = clientId });
        return Ok(result);
    }

    [HttpGet("projects/resource-utilization")]
    [Authorize(Roles = "Administrator,ProjectManager,HR")]
    public async Task<IActionResult> GetResourceUtilization([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] ReportPeriod period = ReportPeriod.ThisMonth)
    {
        var result = await _mediator.Send(new GetResourceUtilizationQuery { StartDate = startDate, EndDate = endDate, Period = period });
        return Ok(result);
    }

    [HttpGet("operational/timesheet-summary")]
    public async Task<IActionResult> GetTimesheetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] long? employeeId)
    {
        var result = await _mediator.Send(new GetTimesheetSummaryQuery
        {
            StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
            EndDate = endDate ?? DateTime.UtcNow,
            EmployeeId = employeeId
        });
        return Ok(result);
    }

    [HttpGet("dashboards/executive")]
    [Authorize(Roles = "Administrator,Executive")]
    public async Task<IActionResult> GetExecutiveDashboard([FromQuery] DateTime? asOfDate)
    {
        var result = await _mediator.Send(new GetExecutiveDashboardQuery { AsOfDate = asOfDate });
        return Ok(result);
    }

    [HttpGet("projects/profitability")]
    [Authorize(Roles = "Administrator,ProjectManager,Finance")]
    public async Task<IActionResult> GetProjectProfitability(
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] long? clientId = null, [FromQuery] string? status = null)
    {
        var result = await _mediator.Send(new GetProjectProfitabilityQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            ClientId = clientId,
            Status = status
        });
        return Ok(result);
    }

    [HttpGet("projects/budget-variance")]
    [Authorize(Roles = "Administrator,ProjectManager,Finance")]
    public async Task<IActionResult> GetBudgetVariance([FromQuery] long projectId)
    {
        var result = await _mediator.Send(new GetBudgetVarianceQuery { ProjectId = projectId });
        return Ok(result);
    }

    [HttpGet("operational/expense-summary")]
    [Authorize(Roles = "Administrator,Finance,HR")]
    public async Task<IActionResult> GetExpenseSummary(
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] long? employeeId = null, [FromQuery] long? projectId = null)
    {
        var result = await _mediator.Send(new GetExpenseSummaryQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            EmployeeId = employeeId,
            ProjectId = projectId
        });
        return Ok(result);
    }

    [HttpGet("dashboards/project-manager")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> GetProjectManagerDashboard([FromQuery] DateTime? asOfDate)
    {
        var result = await _mediator.Send(new GetProjectManagerDashboardQuery { AsOfDate = asOfDate });
        return Ok(result);
    }

    [HttpGet("dashboards/finance")]
    [Authorize(Roles = "Administrator,Finance,AccountingManager")]
    public async Task<IActionResult> GetFinanceDashboard([FromQuery] DateTime? asOfDate)
    {
        var result = await _mediator.Send(new GetFinanceDashboardQuery { AsOfDate = asOfDate });
        return Ok(result);
    }

    [HttpGet("dashboards/employee")]
    [Authorize]
    public async Task<IActionResult> GetEmployeeDashboard([FromQuery] long employeeId)
    {
        var result = await _mediator.Send(new GetEmployeeDashboardQuery { EmployeeId = employeeId });
        return Ok(result);
    }
}
