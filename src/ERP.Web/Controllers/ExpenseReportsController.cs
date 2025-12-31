using ERP.Application.TE.Commands;
using ERP.Application.TE.DTOs;
using ERP.Application.TE.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ExpenseReportsController : ControllerBase
{
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExpenseReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetExpenseReportByIdQuery { ExpenseReportId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetExpenseReportByIdQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseReportCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateExpenseReportCommandHandler>();
        var reportId = await handler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = reportId }, reportId);
    }
}
