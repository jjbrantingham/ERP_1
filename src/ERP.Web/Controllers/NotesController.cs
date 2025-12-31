using ERP.Application.CRM.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Note operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    /// <summary>
    /// Create a new note.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateNoteCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateNoteCommandHandler>();
        var noteId = await handler.Handle(command);
        return CreatedAtAction(nameof(Create), new { id = noteId }, noteId);
    }
}
