using ERP.Application.RPT.DTOs;
using ERP.Application.RPT.Queries;
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

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get Balance Sheet report
    /// </summary>
    [HttpGet("balance-sheet")]
    [ProducesResponseType(typeof(BalanceSheetDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime? asOfDate, [FromQuery] string currency = "USD")
    {
        var query = new GetBalanceSheetQuery
        {
            AsOfDate = asOfDate ?? DateTime.UtcNow,
            Currency = currency
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get Income Statement (Profit & Loss) report
    /// </summary>
    [HttpGet("income-statement")]
    [ProducesResponseType(typeof(IncomeStatementDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncomeStatement(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string currency = "USD")
    {
        var query = new GetIncomeStatementQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            Currency = currency
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get Project Profitability report
    /// </summary>
    [HttpGet("project-profitability")]
    [ProducesResponseType(typeof(ProjectProfitabilityDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectProfitability(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] long? clientId,
        [FromQuery] string? status)
    {
        var query = new GetProjectProfitabilityQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            ClientId = clientId,
            Status = status
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get Timesheet Summary report
    /// </summary>
    [HttpGet("timesheet-summary")]
    [ProducesResponseType(typeof(TimesheetSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimesheetSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] long? employeeId,
        [FromQuery] long? projectId)
    {
        var query = new GetTimesheetSummaryQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            EmployeeId = employeeId,
            ProjectId = projectId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get Invoice Summary report
    /// </summary>
    [HttpGet("invoice-summary")]
    [ProducesResponseType(typeof(InvoiceSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoiceSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] long? clientId,
        [FromQuery] string? status)
    {
        var query = new GetInvoiceSummaryQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            ClientId = clientId,
            Status = status
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
