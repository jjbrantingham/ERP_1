# Create API Endpoint Skill

## Purpose
Generate a complete API endpoint with all layers following Clean Architecture and ASP.NET Core best practices.

## What to Create

### 1. Command or Query (Application Layer)
**File**: `src/ERP.Application/[Context]/Commands/[CommandName].cs` or `Queries/[QueryName].cs`

```csharp
using MediatR;

namespace ERP.Application.[Context].Commands;

public class [CommandName] : IRequest<[ReturnType]>
{
    public [Type] [Property] { get; set; }
    // ... other properties
}
```

### 2. Validator (Application Layer)
**File**: `src/ERP.Application/[Context]/Validators/[CommandName]Validator.cs`

```csharp
using FluentValidation;

namespace ERP.Application.[Context].Validators;

public class [CommandName]Validator : AbstractValidator<[CommandName]>
{
    public [CommandName]Validator()
    {
        RuleFor(x => x.[Property])
            .NotEmpty().WithMessage("[Property] is required")
            .MaximumLength(200).WithMessage("[Property] must not exceed 200 characters");

        RuleFor(x => x.[NumericProperty])
            .GreaterThan(0).WithMessage("[Property] must be positive");
    }
}
```

### 3. Handler (Application Layer)
**File**: `src/ERP.Application/[Context]/Handlers/[CommandName]Handler.cs`

```csharp
using MediatR;
using ERP.Domain.[Context].Repositories;
using ERP.Infrastructure.Persistence;

namespace ERP.Application.[Context].Handlers;

public class [CommandName]Handler : IRequestHandler<[CommandName], [ReturnType]>
{
    private readonly I[Repository] _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ILogger<[CommandName]Handler> _logger;

    public [CommandName]Handler(
        I[Repository] repository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ILogger<[CommandName]Handler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<[ReturnType]> Handle([CommandName] request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {CommandName} for tenant {TenantId}",
            nameof([CommandName]), _currentTenant.TenantId);

        try
        {
            // 1. Validate tenant access to related entities
            // 2. Create/Update domain entity
            // 3. Save changes
            // 4. Return result

            var entity = [DomainEntity].Create(
                _currentTenant.TenantId,
                request.[Property]);

            await _repository.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("{Entity} {Id} created successfully",
                nameof([DomainEntity]), entity.Id);

            return entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {CommandName}", nameof([CommandName]));
            throw;
        }
    }
}
```

### 4. Controller Action (Web Layer)
**File**: `src/ERP.Web/Controllers/[Resource]Controller.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using ERP.Application.[Context].Commands;
using ERP.Application.[Context].Queries;
using ERP.Application.[Context].DTOs;

namespace ERP.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class [Resource]Controller : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<[Resource]Controller> _logger;

    public [Resource]Controller(IMediator mediator, ILogger<[Resource]Controller> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// [Description of what this endpoint does]
    /// </summary>
    /// <param name="command">The command containing [details]</param>
    /// <returns>The ID of the created resource</returns>
    /// <response code="201">Resource created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - insufficient permissions</response>
    [HttpPost]
    [Authorize(Roles = "Administrator,[OtherRoles]")]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] [CommandName] command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Get [resource] by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof([ResourceDto]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new Get[Resource]ByIdQuery { Id = id });
        return Ok(result);
    }

    /// <summary>
    /// Update [resource]
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator,[OtherRoles]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] Update[Resource]Command command)
    {
        command.Id = id;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Delete [resource]
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        await _mediator.Send(new Delete[Resource]Command { Id = id });
        return NoContent();
    }

    /// <summary>
    /// Search [resources] with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<[ResourceDto]>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] Search[Resources]Query query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

### 5. Integration Test (Tests Layer)
**File**: `tests/ERP.IntegrationTests/[Resource]ControllerTests.cs`

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace ERP.IntegrationTests;

public class [Resource]ControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public [Resource]ControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        // Add authentication headers if needed
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var command = new [CommandName]
        {
            [Property] = "[Value]"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/[resource]", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<long>();
        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Create_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var command = new [CommandName]
        {
            [Property] = "" // Invalid
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/[resource]", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ExistingResource_ReturnsOk()
    {
        // Arrange
        var id = await CreateTestResourceAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/[resource]/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<[ResourceDto]>();
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetById_NonExistingResource_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/[resource]/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<long> CreateTestResourceAsync()
    {
        var command = new [CommandName] { [Property] = "Test" };
        var response = await _client.PostAsJsonAsync("/api/v1/[resource]", command);
        return await response.Content.ReadFromJsonAsync<long>();
    }
}
```

## HTTP Methods & Meanings

- **POST**: Create new resource
- **GET**: Retrieve resource(s)
- **PUT**: Full update of resource
- **PATCH**: Partial update of resource
- **DELETE**: Remove resource

## Status Codes

- **200 OK**: Successful GET, PUT, PATCH
- **201 Created**: Successful POST (return Location header)
- **204 No Content**: Successful DELETE, PUT
- **400 Bad Request**: Validation errors
- **401 Unauthorized**: Not authenticated
- **403 Forbidden**: Not authorized
- **404 Not Found**: Resource doesn't exist
- **409 Conflict**: Concurrency conflict
- **500 Internal Server Error**: Unexpected error

## Authorization Roles

Common roles in ERP system:
- `Administrator` - Full system access
- `AccountingManager` - Financial management
- `Finance` - Financial access
- `ProjectManager` - Project management
- `HR` - Human resources access
- `Employee` - Basic employee access

## Checklist

- [ ] Command/Query created with IRequest<T>
- [ ] Validator created with FluentValidation
- [ ] Handler created with proper dependencies
- [ ] Tenant isolation checked in handler
- [ ] Controller action created with proper HTTP verb
- [ ] Swagger documentation (XML comments)
- [ ] Authorization attributes
- [ ] ProducesResponseType attributes
- [ ] Integration tests created
- [ ] Error handling implemented
- [ ] Logging added

## Example Usage

```
User: /create-api-endpoint "POST /api/v1/invoices/generate - Generate invoice from timesheet data"

Claude: I'll create a complete API endpoint for generating invoices...

[Creates all files with proper code]
```

## Related Skills
- create-cqrs-handlers
- generate-integration-tests
- generate-api-docs
