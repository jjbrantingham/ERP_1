using ERP.Application.PM.Commands;
using ERP.Application.PM.DTOs;
using ERP.Application.PM.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Project operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    /// <summary>
    /// Get all projects.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = new GetAllProjectsQuery { ActiveOnly = activeOnly };
        var handler = HttpContext.RequestServices.GetRequiredService<GetAllProjectsQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get project by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetProjectByIdQuery { ProjectId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetProjectByIdQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get project WBS items.
    /// </summary>
    [HttpGet("{id}/wbs")]
    [ProducesResponseType(typeof(IEnumerable<WBSItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWBSItems(long id)
    {
        var query = new GetProjectWBSItemsQuery { ProjectId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetProjectWBSItemsQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get project contracts.
    /// </summary>
    [HttpGet("{id}/contracts")]
    [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContracts(long id)
    {
        var query = new GetProjectContractsQuery { ProjectId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetProjectContractsQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new project.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateProjectCommandHandler>();
        var projectId = await handler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = projectId }, projectId);
    }
}
