using ERP.Application.VM.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Vendor Contact operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VendorContactsController : ControllerBase
{
    /// <summary>
    /// Create a new vendor contact.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVendorContactCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateVendorContactCommandHandler>();
        var contactId = await handler.Handle(command);
        return CreatedAtAction(nameof(Create), new { id = contactId }, contactId);
    }
}
