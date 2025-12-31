using ERP.Application.PM.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for WBS Item operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WBSItemsController : ControllerBase
{
    /// <summary>
    /// Create a new WBS item.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWBSItemCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateWBSItemCommandHandler>();
        var wbsItemId = await handler.Handle(command);
        return CreatedAtAction(nameof(Create), new { id = wbsItemId }, wbsItemId);
    }
}
