# Create CQRS Command and Query Handlers

Generate complete CQRS command and query handlers following the ERP application patterns.

## What This Skill Does

Creates commands or queries with:

1. **Command/Query class**
2. **Handler implementation**
3. **FluentValidation validator**
4. **Unit tests**
5. **Controller endpoint** (optional)

## Expected Input

Ask the user:
- **Type**: Command or Query?
- **Operation name** (e.g., "CreateInvoice", "GetInvoiceById", "ApproveTimesheet")
- **Bounded context** (e.g., "Billing", "TE", "PM")
- **Parameters** needed
- **Return type** (for queries) or result type (for commands)
- **Business rules** to validate

## Command Pattern

### Command Class

Create in `src/ERP.Application/{Context}/Commands/{CommandName}Command.cs`:

```csharp
using MediatR;

namespace ERP.Application.{Context}.Commands;

/// <summary>
/// {Description of what this command does}
/// </summary>
public class {CommandName}Command : IRequest<{ReturnType}>
{
    public long Id { get; set; }
    public string PropertyName { get; set; }
    // Other properties
}
```

### Command Validator

Create in `src/ERP.Application/{Context}/Commands/{CommandName}CommandValidator.cs`:

```csharp
using FluentValidation;

namespace ERP.Application.{Context}.Commands;

public class {CommandName}CommandValidator : AbstractValidator<{CommandName}Command>
{
    public {CommandName}CommandValidator()
    {
        RuleFor(x => x.PropertyName)
            .NotEmpty().WithMessage("Property is required")
            .MaximumLength(100).WithMessage("Property must not exceed 100 characters");

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than zero");

        // Add more validation rules
    }
}
```

### Command Handler

Create in `src/ERP.Application/{Context}/Commands/{CommandName}CommandHandler.cs`:

```csharp
using ERP.Domain.{Context}.Entities;
using ERP.Domain.{Context}.Repositories;
using ERP.Domain.Common.Interfaces;
using ERP.Application.Common.Interfaces;
using MediatR;

namespace ERP.Application.{Context}.Commands;

public class {CommandName}CommandHandler : IRequestHandler<{CommandName}Command, {ReturnType}>
{
    private readonly I{Aggregate}Repository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public {CommandName}CommandHandler(
        I{Aggregate}Repository repository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<{ReturnType}> Handle({CommandName}Command request, CancellationToken cancellationToken)
    {
        // 1. Validate business rules
        // 2. Get or create entity
        // 3. Apply business logic
        // 4. Save changes
        // 5. Return result

        var entity = {Aggregate}.Create(
            _currentTenant.TenantId,
            request.PropertyName
            // other parameters
        );

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id; // or appropriate return value
    }
}
```

## Query Pattern

### Query Class

Create in `src/ERP.Application/{Context}/Queries/{QueryName}Query.cs`:

```csharp
using MediatR;
using ERP.Application.{Context}.DTOs;

namespace ERP.Application.{Context}.Queries;

/// <summary>
/// {Description of what this query retrieves}
/// </summary>
public class {QueryName}Query : IRequest<{ReturnType}>
{
    public long Id { get; set; }
    // Other filter parameters
}
```

### Query Validator

Create in `src/ERP.Application/{Context}/Queries/{QueryName}QueryValidator.cs`:

```csharp
using FluentValidation;

namespace ERP.Application.{Context}.Queries;

public class {QueryName}QueryValidator : AbstractValidator<{QueryName}Query>
{
    public {QueryName}QueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than zero");

        // Add validation rules
    }
}
```

### Query Handler

Create in `src/ERP.Application/{Context}/Queries/{QueryName}QueryHandler.cs`:

```csharp
using AutoMapper;
using ERP.Domain.{Context}.Repositories;
using ERP.Application.{Context}.DTOs;
using ERP.Application.Common.Exceptions;
using MediatR;

namespace ERP.Application.{Context}.Queries;

public class {QueryName}QueryHandler : IRequestHandler<{QueryName}Query, {ReturnType}>
{
    private readonly I{Aggregate}Repository _repository;
    private readonly IMapper _mapper;

    public {QueryName}QueryHandler(
        I{Aggregate}Repository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<{ReturnType}> Handle({QueryName}Query request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("{Aggregate} not found");

        return _mapper.Map<{ReturnType}>(entity);
    }
}
```

## Common Command Examples

### Create Command

```csharp
public class Create{Aggregate}Command : IRequest<long>
{
    public string Name { get; set; }
    // Properties for creation
}

// Handler creates new entity
var entity = {Aggregate}.Create(_currentTenant.TenantId, request.Name, ...);
await _repository.AddAsync(entity, cancellationToken);
```

### Update Command

```csharp
public class Update{Aggregate}Command : IRequest
{
    public long Id { get; set; }
    public string Name { get; set; }
    // Properties to update
}

// Handler retrieves and updates
var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
if (entity == null) throw new NotFoundException();
entity.UpdateName(request.Name); // Use business methods!
```

### Delete Command

```csharp
public class Delete{Aggregate}Command : IRequest
{
    public long Id { get; set; }
}

// Handler retrieves and deletes
var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
if (entity == null) throw new NotFoundException();
await _repository.DeleteAsync(entity, cancellationToken);
```

### Approve/Reject Commands

```csharp
public class Approve{Aggregate}Command : IRequest
{
    public long Id { get; set; }
    public string Comments { get; set; }
}

// Handler uses domain method
var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
if (entity == null) throw new NotFoundException();
entity.Approve(_currentUser.UserId, request.Comments); // Domain method raises events
```

## Common Query Examples

### Get by ID Query

```csharp
public class Get{Aggregate}ByIdQuery : IRequest<{Aggregate}Dto>
{
    public long Id { get; set; }
}

// Handler retrieves and maps
var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
return _mapper.Map<{Aggregate}Dto>(entity);
```

### Get All/List Query

```csharp
public class Get{Aggregate}ListQuery : IRequest<IEnumerable<{Aggregate}Dto>>
{
    public string? Filter { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// Handler with pagination
var entities = await _repository.GetPagedAsync(request.Page, request.PageSize, request.Filter, cancellationToken);
return _mapper.Map<IEnumerable<{Aggregate}Dto>>(entities);
```

## Register in DI

Update `src/ERP.Web/Program.cs`:

```csharp
// MediatR automatically registers all handlers in the assembly
// Just ensure the assembly is registered:
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Create{Aggregate}Command).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Create{Aggregate}CommandValidator).Assembly);
```

## Unit Tests

Create in `tests/ERP.UnitTests/{Context}/Commands/{CommandName}CommandHandlerTests.cs`:

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using ERP.Application.{Context}.Commands;
using ERP.Domain.{Context}.Repositories;
using ERP.Domain.Common.Interfaces;
using ERP.Application.Common.Interfaces;

namespace ERP.UnitTests.{Context}.Commands;

public class {CommandName}CommandHandlerTests
{
    private readonly Mock<I{Aggregate}Repository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentTenantService> _tenantServiceMock;
    private readonly {CommandName}CommandHandler _handler;

    public {CommandName}CommandHandlerTests()
    {
        _repositoryMock = new Mock<I{Aggregate}Repository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantServiceMock = new Mock<ICurrentTenantService>();

        _tenantServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        _handler = new {CommandName}CommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _tenantServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_Should{ExpectedOutcome}()
    {
        // Arrange
        var command = new {CommandName}Command
        {
            PropertyName = "Test Value"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<{Aggregate}>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ShouldThrowException()
    {
        // Arrange
        var command = new {CommandName}Command
        {
            // Invalid data
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
```

## Validation Checklist

- [ ] Command/Query class created with IRequest
- [ ] Validator created with FluentValidation
- [ ] Handler implements IRequestHandler
- [ ] Multi-tenancy checked (_currentTenant.TenantId)
- [ ] Authorization verified if needed
- [ ] Exceptions properly thrown (NotFoundException, etc.)
- [ ] Unit tests created
- [ ] Domain methods used (not property setters)
- [ ] Unit of work pattern used for transactions

## Best Practices

- Commands change state, queries don't
- Commands return void or ID, queries return DTOs
- Always validate in both validator and handler
- Use domain methods, not property setters
- Check authorization before business logic
- Throw specific exceptions (NotFoundException, etc.)
- Use async/await consistently
- Include XML documentation comments
- Write comprehensive unit tests
