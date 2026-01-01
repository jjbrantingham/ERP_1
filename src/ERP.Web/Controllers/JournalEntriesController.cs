using ERP.Application.FIN.Commands;
using ERP.Application.FIN.DTOs;
using ERP.Application.FIN.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public JournalEntriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get journal entry by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(JournalEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetJournalEntryByIdQuery { JournalEntryId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new journal entry
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateJournalEntryCommand command)
    {
        var journalEntryId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = journalEntryId }, journalEntryId);
    }

    /// <summary>
    /// Post a journal entry to the general ledger
    /// </summary>
    [HttpPost("{id}/post")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(long id)
    {
        var command = new PostJournalEntryCommand { JournalEntryId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
