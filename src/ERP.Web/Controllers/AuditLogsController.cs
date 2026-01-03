using ERP.Application.AUDIT.Commands;
using ERP.Application.AUDIT.DTOs;
using ERP.Application.AUDIT.Queries;
using ERP.Application.Common.Models;
using ERP.Domain.AUDIT.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for audit log management.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get audit log entry by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetAuditLogByIdQuery { AuditLogId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Search audit logs with filters.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] AuditEventType? eventType = null,
        [FromQuery] AuditSeverity? severity = null,
        [FromQuery] long? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new SearchAuditLogsQuery
        {
            EntityType = entityType,
            EntityId = entityId,
            EventType = eventType,
            Severity = severity,
            UserId = userId,
            FromDate = fromDate,
            ToDate = toDate,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get complete audit trail for a specific entity.
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityAuditTrail(string entityType, long entityId)
    {
        var query = new GetAuditTrailForEntityQuery
        {
            EntityType = entityType,
            EntityId = entityId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get activity logs for a specific user.
    /// </summary>
    [HttpGet("user/{userId}/activity")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserActivity(
        long userId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetUserActivityQuery
        {
            UserId = userId,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Manually create an audit log entry (for special cases).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,AuditManager")]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogCommand command)
    {
        // Capture IP address and user agent from request
        command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        command.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var auditLogId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = auditLogId }, auditLogId);
    }

    /// <summary>
    /// Purge audit logs older than a specified date (for data retention compliance).
    /// Requires Administrator role.
    /// </summary>
    [HttpDelete("purge")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PurgeOldLogs(
        [FromQuery] DateTime olderThan,
        [FromQuery] bool dryRun = false)
    {
        var command = new PurgeOldAuditLogsCommand
        {
            OlderThan = olderThan,
            DryRun = dryRun
        };

        var count = await _mediator.Send(command);

        var message = dryRun
            ? $"Dry run: {count} audit logs would be deleted"
            : $"{count} audit logs were deleted";

        return Ok(new { count, message, dryRun });
    }

    /// <summary>
    /// Get recent audit logs (last 100 by default).
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 100)
    {
        var query = new SearchAuditLogsQuery
        {
            PageNumber = 1,
            PageSize = Math.Min(count, 500) // Cap at 500
        };

        var result = await _mediator.Send(query);
        return Ok(result.Items);
    }

    /// <summary>
    /// Get financial audit logs for compliance reporting.
    /// </summary>
    [HttpGet("financial")]
    [Authorize(Roles = "Administrator,AccountingManager,Auditor")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFinancialAuditLogs(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100)
    {
        var query = new SearchAuditLogsQuery
        {
            EventType = AuditEventType.FinancialPost, // Can be extended to include other financial events
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get security audit logs (login attempts, permission changes).
    /// </summary>
    [HttpGet("security")]
    [Authorize(Roles = "Administrator,SecurityManager")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSecurityAuditLogs(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100)
    {
        // This would need to be extended to query multiple event types
        var query = new SearchAuditLogsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        // Filter for security-related events
        var securityEvents = new[]
        {
            AuditEventType.Login,
            AuditEventType.Logout,
            AuditEventType.LoginFailed,
            AuditEventType.PasswordChanged,
            AuditEventType.PermissionChanged
        };

        var filteredItems = result.Items.Where(a => securityEvents.Contains(a.EventType)).ToList();

        return Ok(new PagedResult<AuditLogDto>(filteredItems, filteredItems.Count, pageNumber, pageSize));
    }
}
