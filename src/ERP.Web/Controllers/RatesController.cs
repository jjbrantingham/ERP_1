using ERP.Application.HR.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Rate operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RatesController : ControllerBase
{
    /// <summary>
    /// Create a new rate.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRateCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateRateCommandHandler>();
        var rateId = await handler.Handle(command);
        return CreatedAtAction(nameof(Create), new { id = rateId }, rateId);
    }
}
