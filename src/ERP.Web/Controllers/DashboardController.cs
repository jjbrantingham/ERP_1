using ERP.Application.DASH.DTOs;
using ERP.Application.DASH.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// Dashboard endpoints for various user roles
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get executive dashboard with high-level KPIs
    /// </summary>
    /// <param name="asOfDate">As of date (defaults to today)</param>
    /// <param name="currency">Currency code (defaults to USD)</param>
    /// <returns>Executive dashboard data</returns>
    [HttpGet("executive")]
    [ProducesResponseType(typeof(ExecutiveDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetExecutiveDashboard(
        [FromQuery] DateTime? asOfDate = null,
        [FromQuery] string currency = "USD")
    {
        try
        {
            var query = new GetExecutiveDashboardQuery
            {
                AsOfDate = asOfDate,
                Currency = currency
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving executive dashboard");
            return StatusCode(500, "An error occurred while retrieving the executive dashboard");
        }
    }

    /// <summary>
    /// Get financial dashboard with detailed financial metrics
    /// </summary>
    /// <param name="asOfDate">As of date (defaults to today)</param>
    /// <param name="currency">Currency code (defaults to USD)</param>
    /// <returns>Financial dashboard data</returns>
    [HttpGet("financial")]
    [ProducesResponseType(typeof(FinancialDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFinancialDashboard(
        [FromQuery] DateTime? asOfDate = null,
        [FromQuery] string currency = "USD")
    {
        try
        {
            var query = new GetFinancialDashboardQuery
            {
                AsOfDate = asOfDate,
                Currency = currency
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving financial dashboard");
            return StatusCode(500, "An error occurred while retrieving the financial dashboard");
        }
    }

    /// <summary>
    /// Get project dashboard with project portfolio metrics
    /// </summary>
    /// <param name="asOfDate">As of date (defaults to today)</param>
    /// <param name="currency">Currency code (defaults to USD)</param>
    /// <returns>Project dashboard data</returns>
    [HttpGet("projects")]
    [ProducesResponseType(typeof(ProjectDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProjectDashboard(
        [FromQuery] DateTime? asOfDate = null,
        [FromQuery] string currency = "USD")
    {
        try
        {
            var query = new GetProjectDashboardQuery
            {
                AsOfDate = asOfDate,
                Currency = currency
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project dashboard");
            return StatusCode(500, "An error occurred while retrieving the project dashboard");
        }
    }

    /// <summary>
    /// Get employee dashboard with personal metrics and tasks
    /// </summary>
    /// <param name="employeeId">Employee ID (defaults to current user's employee)</param>
    /// <param name="asOfDate">As of date (defaults to today)</param>
    /// <returns>Employee dashboard data</returns>
    [HttpGet("employee")]
    [ProducesResponseType(typeof(EmployeeDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmployeeDashboard(
        [FromQuery] long? employeeId = null,
        [FromQuery] DateTime? asOfDate = null)
    {
        try
        {
            var query = new GetEmployeeDashboardQuery
            {
                EmployeeId = employeeId,
                AsOfDate = asOfDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee dashboard");
            return StatusCode(500, "An error occurred while retrieving the employee dashboard");
        }
    }

    /// <summary>
    /// Get dashboard based on user role (smart routing)
    /// </summary>
    /// <returns>Role-appropriate dashboard data</returns>
    [HttpGet("my-dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyDashboard()
    {
        try
        {
            // Determine user role and return appropriate dashboard
            // This is simplified - would check actual user roles/claims

            var userRoles = User.Claims
                .Where(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .Select(c => c.Value)
                .ToList();

            if (userRoles.Contains("Executive") || userRoles.Contains("CEO") || userRoles.Contains("Admin"))
            {
                var query = new GetExecutiveDashboardQuery();
                var result = await _mediator.Send(query);
                return Ok(new { DashboardType = "Executive", Data = result });
            }
            else if (userRoles.Contains("Finance") || userRoles.Contains("CFO") || userRoles.Contains("Accountant"))
            {
                var query = new GetFinancialDashboardQuery();
                var result = await _mediator.Send(query);
                return Ok(new { DashboardType = "Financial", Data = result });
            }
            else if (userRoles.Contains("ProjectManager") || userRoles.Contains("PM"))
            {
                var query = new GetProjectDashboardQuery();
                var result = await _mediator.Send(query);
                return Ok(new { DashboardType = "Project", Data = result });
            }
            else
            {
                // Default to employee dashboard
                var query = new GetEmployeeDashboardQuery();
                var result = await _mediator.Send(query);
                return Ok(new { DashboardType = "Employee", Data = result });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard");
            return StatusCode(500, "An error occurred while retrieving the dashboard");
        }
    }
}
