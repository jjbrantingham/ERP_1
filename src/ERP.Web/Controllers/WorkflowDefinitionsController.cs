using ERP.Application.WF.Commands;
using ERP.Application.WF.DTOs;
using ERP.Application.WF.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// Controller for managing workflow definitions (admin/configuration)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WorkflowDefinitionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkflowDefinitionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get workflow definition by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(WorkflowDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetWorkflowDefinitionByIdQuery { WorkflowDefinitionId = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get workflow definitions by entity type (e.g., "Timesheet", "Invoice")
    /// </summary>
    [HttpGet("entity-type/{entityType}")]
    [ProducesResponseType(typeof(IEnumerable<WorkflowDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntityType(string entityType, [FromQuery] bool activeOnly = true)
    {
        var query = new GetWorkflowDefinitionsByEntityTypeQuery
        {
            EntityType = entityType,
            ActiveOnly = activeOnly
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new workflow definition
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowDefinitionCommand command)
    {
        var workflowId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = workflowId }, workflowId);
    }

    /// <summary>
    /// Activate a workflow definition
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate(long id)
    {
        var command = new ActivateWorkflowDefinitionCommand { WorkflowDefinitionId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Deactivate a workflow definition
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(long id)
    {
        var command = new DeactivateWorkflowDefinitionCommand { WorkflowDefinitionId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
