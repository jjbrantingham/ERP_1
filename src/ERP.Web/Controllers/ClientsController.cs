using ERP.Application.CRM.Commands;
using ERP.Application.CRM.DTOs;
using ERP.Application.CRM.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Client operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    /// <summary>
    /// Get all clients.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = new GetAllClientsQuery { ActiveOnly = activeOnly };
        var handler = HttpContext.RequestServices.GetRequiredService<GetAllClientsQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get client by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetClientByIdQuery { ClientId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetClientByIdQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get client contacts.
    /// </summary>
    [HttpGet("{id}/contacts")]
    [ProducesResponseType(typeof(IEnumerable<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(long id)
    {
        var query = new GetClientContactsQuery { ClientId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetClientContactsQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get client notes.
    /// </summary>
    [HttpGet("{id}/notes")]
    [ProducesResponseType(typeof(IEnumerable<NoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotes(long id)
    {
        var query = new GetClientNotesQuery { ClientId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetClientNotesQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new client.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClientCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateClientCommandHandler>();
        var clientId = await handler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = clientId }, clientId);
    }
}
