using ERP.Application.HR.Commands;
using ERP.Application.HR.DTOs;
using ERP.Application.HR.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

/// <summary>
/// API controller for Employee operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    /// <summary>
    /// Get all employees.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var query = new GetAllEmployeesQuery { ActiveOnly = activeOnly };
        var handler = HttpContext.RequestServices.GetRequiredService<GetAllEmployeesQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get employee by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetEmployeeByIdQuery { EmployeeId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetEmployeeByIdQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Get employee rates.
    /// </summary>
    [HttpGet("{id}/rates")]
    [ProducesResponseType(typeof(IEnumerable<RateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRates(long id)
    {
        var query = new GetEmployeeRatesQuery { EmployeeId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<GetEmployeeRatesQueryHandler>();
        var result = await handler.Handle(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new employee.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand command)
    {
        var handler = HttpContext.RequestServices.GetRequiredService<CreateEmployeeCommandHandler>();
        var employeeId = await handler.Handle(command);
        return CreatedAtAction(nameof(GetById), new { id = employeeId }, employeeId);
    }

    /// <summary>
    /// Update an employee.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateEmployeeCommand command)
    {
        var updateCommand = new UpdateEmployeeCommand
        {
            EmployeeId = id,
            ResourceTypeId = command.ResourceTypeId,
            FirstName = command.FirstName,
            LastName = command.LastName,
            MiddleName = command.MiddleName,
            Email = command.Email,
            PhoneNumber = command.PhoneNumber,
            MobileNumber = command.MobileNumber,
            DateOfBirth = command.DateOfBirth,
            EmploymentType = command.EmploymentType,
            JobTitle = command.JobTitle,
            Department = command.Department,
            ManagerId = command.ManagerId,
            BaseSalaryAmount = command.BaseSalaryAmount,
            BaseSalaryCurrency = command.BaseSalaryCurrency,
            StandardHoursPerWeek = command.StandardHoursPerWeek,
            Notes = command.Notes
        };
        var handler = HttpContext.RequestServices.GetRequiredService<UpdateEmployeeCommandHandler>();
        await handler.Handle(updateCommand);
        return NoContent();
    }
}
