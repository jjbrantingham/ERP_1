using ERP.Application.PM.Commands;
using ERP.Application.PM.DTOs;
using ERP.Application.PM.Queries;
using MediatR;
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
    private readonly IMediator _mediator;

    public ContractsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all contracts with project and client information.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContractListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = false,
        [FromQuery] string? contractType = null,
        [FromQuery] long? clientId = null)
    {
        var query = new GetAllContractsQuery
        {
            ActiveOnly = activeOnly,
            ContractType = contractType,
            ClientId = clientId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get contract by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContractDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetContractByIdQuery { ContractId = id };
        var result = await _mediator.Send(query);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Create a new contract.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateContractCommand command)
    {
        var contractId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = contractId }, contractId);
    }

    /// <summary>
    /// Update an existing contract.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateContractCommand command)
    {
        // Reconstruct command with ID from route
        var updateCommand = new UpdateContractCommand
        {
            Id = id,
            Title = command.Title,
            Description = command.Description,
            ContractValueAmount = command.ContractValueAmount,
            ContractValueCurrency = command.ContractValueCurrency,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            SignedDate = command.SignedDate,
            Terms = command.Terms
        };
        await _mediator.Send(updateCommand);
        return NoContent();
    }

    /// <summary>
    /// Delete a contract.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var command = new DeleteContractCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
