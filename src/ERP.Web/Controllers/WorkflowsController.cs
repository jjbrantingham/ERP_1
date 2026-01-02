using ERP.Application.WF.Commands;
using ERP.Application.WF.DTOs;
using ERP.Application.WF.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// Controller for managing workflow instances (runtime execution and approvals)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkflowsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get workflow instance by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkflowInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetWorkflowInstanceByIdQuery { WorkflowInstanceId = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get workflow instance by entity (e.g., Timesheet ID 123)
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(WorkflowInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEntity(string entityType, long entityId)
    {
        var query = new GetWorkflowInstanceByEntityQuery
        {
            EntityType = entityType,
            EntityId = entityId
        };

        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get pending approvals for current user
    /// </summary>
    [HttpGet("pending-approvals")]
    [ProducesResponseType(typeof(IEnumerable<WorkflowInstanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingApprovals([FromQuery] long? userId = null, [FromQuery] string? role = null)
    {
        var query = new GetPendingApprovalsQuery
        {
            UserId = userId,
            Role = role
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Start a new workflow for an entity
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartWorkflow([FromBody] StartWorkflowCommand command)
    {
        var workflowInstanceId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = workflowInstanceId }, workflowInstanceId);
    }

    /// <summary>
    /// Approve a workflow step
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveStep(long id, [FromBody] ApproveStepRequest request)
    {
        var command = new ApproveStepCommand
        {
            WorkflowInstanceId = id,
            StepSequenceNumber = request.StepSequenceNumber,
            Comments = request.Comments
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Reject a workflow step
    /// </summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RejectStep(long id, [FromBody] RejectStepRequest request)
    {
        var command = new RejectStepCommand
        {
            WorkflowInstanceId = id,
            StepSequenceNumber = request.StepSequenceNumber,
            Comments = request.Comments
        };

        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Cancel a workflow
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(long id, [FromBody] CancelWorkflowRequest request)
    {
        var command = new CancelWorkflowCommand
        {
            WorkflowInstanceId = id,
            Reason = request.Reason
        };

        await _mediator.Send(command);
        return NoContent();
    }
}

/// <summary>
/// Request model for approving a step
/// </summary>
public class ApproveStepRequest
{
    public int StepSequenceNumber { get; set; }
    public string? Comments { get; set; }
}

/// <summary>
/// Request model for rejecting a step
/// </summary>
public class RejectStepRequest
{
    public int StepSequenceNumber { get; set; }
    public string Comments { get; set; } = string.Empty;
}

/// <summary>
/// Request model for cancelling a workflow
/// </summary>
public class CancelWorkflowRequest
{
    public string Reason { get; set; } = string.Empty;
}
