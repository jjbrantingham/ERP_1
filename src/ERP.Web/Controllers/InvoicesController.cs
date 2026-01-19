using ERP.Application.BILL.Commands;
using ERP.Application.BILL.DTOs;
using ERP.Application.BILL.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetInvoiceByIdQuery { InvoiceId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand command)
    {
        var invoiceId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = invoiceId }, invoiceId);
    }

    /// <summary>
    /// Post invoice to accounting system
    /// </summary>
    [HttpPost("{id}/post")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post(long id)
    {
        var command = new PostInvoiceCommand { InvoiceId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Apply payment to invoice
    /// </summary>
    [HttpPost("{id}/payments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyPayment(long id, [FromBody] ApplyPaymentCommand command)
    {
        var applyPaymentCommand = new ApplyPaymentCommand
        {
            InvoiceId = id,
            PaymentAmount = command.PaymentAmount,
            Currency = command.Currency
        };
        await _mediator.Send(applyPaymentCommand);
        return NoContent();
    }
}
