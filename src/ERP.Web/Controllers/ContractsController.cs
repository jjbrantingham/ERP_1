using ERP.Application.PM.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Contract operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    /// <summary>
    /// Create a new contract.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateContractCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateContractCommandHandler>();
        var contractId = await handler.Handle(command);
        return CreatedAtAction(nameof(Create), new { id = contractId }, contractId);
    }
}
