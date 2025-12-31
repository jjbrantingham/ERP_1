using ERP.Application.VM.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Vendor Note operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VendorNotesController : ControllerBase
{
    /// <summary>
    /// Create a new vendor note.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVendorNoteCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateVendorNoteCommandHandler>();
        var noteId = await handler.Handle(command);
        return CreatedAtAction(nameof(Create), new { id = noteId }, noteId);
    }
}
