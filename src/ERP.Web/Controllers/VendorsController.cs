using ERP.Application.VM.Commands;
using ERP.Application.VM.DTOs;
using ERP.Application.VM.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Vendor operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VendorsController : ControllerBase
{
    /// <summary>
    /// Get all vendors.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VendorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = new GetAllVendorsQuery { ActiveOnly = activeOnly };
        var handler = HttpContext.RequestServices.GetRequiredService<GetAllVendorsQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get vendor by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetVendorByIdQuery { VendorId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetVendorByIdQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get vendor contacts.
    /// </summary>
    [HttpGet("{id}/contacts")]
    [ProducesResponseType(typeof(IEnumerable<VendorContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(long id)
    {
        var query = new GetVendorContactsQuery { VendorId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetVendorContactsQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get vendor notes.
    /// </summary>
    [HttpGet("{id}/notes")]
    [ProducesResponseType(typeof(IEnumerable<VendorNoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotes(long id)
    {
        var query = new GetVendorNotesQuery { VendorId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetVendorNotesQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new vendor.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateVendorCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateVendorCommandHandler>();
        var vendorId = await handler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = vendorId }, vendorId);
    }
}
