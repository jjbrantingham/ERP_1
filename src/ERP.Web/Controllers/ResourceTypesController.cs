using ERP.Application.HR.Commands;
using ERP.Application.HR.DTOs;
using ERP.Application.HR.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for ResourceType operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ResourceTypesController : ControllerBase
{
    /// <summary>
    /// Get all resource types.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ResourceTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = new GetAllResourceTypesQuery { ActiveOnly = activeOnly };
        var handler = HttpContext.RequestServices.GetRequiredService<GetAllResourceTypesQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new resource type.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateResourceTypeCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateResourceTypeCommandHandler>();
        var resourceTypeId = await handler.Handle(command);
        return CreatedAtAction(nameof(GetAll), new { }, resourceTypeId);
    }
}
