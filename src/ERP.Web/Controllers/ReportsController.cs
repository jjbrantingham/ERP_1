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
        var result = await _mediator.Send(new GetBalanceSheetQuery { AsOfDate = asOfDate });
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
        var result = await _mediator.Send(new GetTimesheetSummaryQuery { StartDate = startDate, EndDate = endDate, EmployeeId = employeeId });
        return Ok(result);
    }

    [HttpGet("dashboards/executive")]
    [Authorize(Roles = "Administrator,Executive")]
    public async Task<IActionResult> GetExecutiveDashboard([FromQuery] DateTime? asOfDate)
    {
        var result = await _mediator.Send(new GetExecutiveDashboardQuery { AsOfDate = asOfDate });
        return Ok(result);
    }
}
