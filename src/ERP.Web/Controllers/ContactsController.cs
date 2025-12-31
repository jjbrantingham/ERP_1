using ERP.Application.CRM.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Contact operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ContactsController : ControllerBase
{
    /// <summary>
    /// Create a new contact.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateContactCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateContactCommandHandler>();
        var contactId = await handler.Handle(command);
        return CreatedAtAction(nameof(Create), new { id = contactId }, contactId);
    }
}
