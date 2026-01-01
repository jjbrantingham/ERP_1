# Code Review

Comprehensive code review for ERP application following DDD, SOLID, and security best practices.

## What This Skill Does

Performs thorough code review checking:

1. **Architecture & Design**
2. **SOLID Principles**
3. **DDD Patterns**
4. **Security**
5. **Performance**
6. **Testing**
7. **Code Quality**
8. **Multi-Tenancy**
9. **Error Handling**
10. **Documentation**

## Review Checklist

### 1. Architecture & Design

✅ **Layer Separation**:
- Domain logic in Domain layer (not Application or Infrastructure)
- No infrastructure dependencies in Domain layer
- Application layer orchestrates, doesn't contain business logic
- Controllers are thin, delegate to MediatR

✅ **Dependency Direction**:
- Dependencies point inward (Infrastructure → Application → Domain)
- Domain has no dependencies on other layers
- Interfaces in Domain, implementations in Infrastructure

### 2. SOLID Principles

✅ **Single Responsibility Principle (SRP)**:
- Each class has one reason to change
- Commands/queries do one thing
- Handlers have single responsibility

✅ **Open/Closed Principle (OCP)**:
- Open for extension, closed for modification
- Use interfaces and inheritance appropriately

✅ **Liskov Substitution Principle (LSP)**:
- Derived classes substitutable for base classes
- Interface implementations work as expected

✅ **Interface Segregation Principle (ISP)**:
- Interfaces are focused and minimal
- No fat interfaces with unused methods

✅ **Dependency Inversion Principle (DIP)**:
- Depend on abstractions, not concretions
- Use dependency injection throughout

### 3. Domain-Driven Design

✅ **Aggregates**:
```csharp
// ✅ GOOD: Aggregate with encapsulation
public class Order : AggregateRoot
{
    private readonly List<OrderItem> _items;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Business methods, not property setters
    public void AddItem(Product product, int quantity)
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("Cannot modify shipped orders");

        _items.Add(new OrderItem(product, quantity));
        RecalculateTotal();
    }
}

// ❌ BAD: Public setters, no encapsulation
public class Order
{
    public List<OrderItem> Items { get; set; }  // BAD: Mutable collection
    public decimal Total { get; set; }          // BAD: Can be set directly
}
```

✅ **Value Objects**:
```csharp
// ✅ GOOD: Immutable value object
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
}

// ❌ BAD: Mutable "value object"
public class Money
{
    public decimal Amount { get; set; }  // BAD: Mutable
}
```

✅ **Domain Events**:
```csharp
// ✅ GOOD: Domain events for state changes
public void Submit()
{
    Status = TimesheetStatus.Submitted;
    AddDomainEvent(new TimesheetSubmittedEvent(Id, TenantId));
}

// ❌ BAD: No domain events
public void Submit()
{
    Status = TimesheetStatus.Submitted;  // Silent state change
}
```

✅ **Factory Methods**:
```csharp
// ✅ GOOD: Factory method with validation
public static Invoice Create(Guid tenantId, string invoiceNumber, ...)
{
    if (string.IsNullOrWhiteSpace(invoiceNumber))
        throw new ArgumentException("Invoice number required");

    return new Invoice { TenantId = tenantId, ... };
}

// ❌ BAD: Public parameterless constructor
public Invoice() { }  // Allows invalid state
```

### 4. Security

✅ **Input Validation**:
```csharp
// ✅ GOOD: Validation in multiple layers
public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ClientId).GreaterThan(0);
    }
}

public class CreateInvoiceCommandHandler
{
    public async Task<long> Handle(CreateInvoiceCommand request, ...)
    {
        // Additional business validation
        var client = await _clientRepository.GetByIdAsync(request.ClientId);
        if (client == null)
            throw new NotFoundException("Client not found");

        // Verify client belongs to current tenant (authorization)
        if (client.TenantId != _currentTenant.TenantId)
            throw new UnauthorizedAccessException();
    }
}
```

✅ **SQL Injection Prevention**:
```csharp
// ✅ GOOD: Parameterized queries (EF Core does this)
var invoices = await _context.Invoices
    .Where(x => x.ClientId == clientId)
    .ToListAsync();

// ❌ DANGEROUS: String concatenation in raw SQL
var sql = $"SELECT * FROM Invoices WHERE ClientId = {clientId}";  // NEVER!
```

✅ **XSS Prevention**:
```csharp
// ✅ GOOD: Razor automatically encodes
<h1>@Model.InvoiceNumber</h1>

// ⚠️ DANGEROUS: Only use Html.Raw for trusted content
@Html.Raw(Model.Description)  // Review carefully!
```

✅ **Authorization**:
```csharp
// ✅ GOOD: Authorization checks
[Authorize]
[HttpGet("{id}")]
public async Task<IActionResult> GetById(long id)
{
    // Multi-tenancy check happens in repository via global filter
}

// ❌ BAD: No authorization
[HttpGet("{id}")]  // Missing [Authorize]
```

✅ **Sensitive Data**:
```csharp
// ✅ GOOD: Don't log sensitive data
_logger.LogInformation("Invoice {InvoiceId} created", invoice.Id);

// ❌ BAD: Logging sensitive data
_logger.LogInformation("User password: {Password}", password);  // NEVER!
_logger.LogInformation("SSN: {SSN}", employee.SSN);            // NEVER!
```

### 5. Performance

✅ **N+1 Query Prevention**:
```csharp
// ✅ GOOD: Eager loading
var invoices = await _context.Invoices
    .Include(x => x.LineItems)
    .Include(x => x.Client)
    .ToListAsync();

// ❌ BAD: N+1 queries
var invoices = await _context.Invoices.ToListAsync();
foreach (var invoice in invoices)
{
    var client = await _context.Clients.FindAsync(invoice.ClientId);  // N+1!
}
```

✅ **Projections**:
```csharp
// ✅ GOOD: Project to DTO
var dtos = await _context.Invoices
    .Select(x => new InvoiceDto
    {
        Id = x.Id,
        InvoiceNumber = x.InvoiceNumber,
        // Only fields needed
    })
    .ToListAsync();

// ❌ BAD: Load full entities when only need a few fields
var invoices = await _context.Invoices.ToListAsync();
var dtos = invoices.Select(x => new InvoiceDto { ... });
```

✅ **Pagination**:
```csharp
// ✅ GOOD: Paginated queries
public async Task<PagedResult<InvoiceDto>> GetPagedAsync(int page, int pageSize)
{
    var query = _context.Invoices.AsQueryable();
    var total = await query.CountAsync();

    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<InvoiceDto>(items, total, page, pageSize);
}

// ❌ BAD: Load all records
public async Task<List<Invoice>> GetAllAsync()
{
    return await _context.Invoices.ToListAsync();  // Could be millions!
}
```

✅ **Async/Await**:
```csharp
// ✅ GOOD: Async I/O operations
public async Task<Invoice> GetByIdAsync(long id)
{
    return await _context.Invoices.FirstOrDefaultAsync(x => x.Id == id);
}

// ❌ BAD: Blocking I/O
public Invoice GetById(long id)
{
    return _context.Invoices.FirstOrDefault(x => x.Id == id);  // Blocks thread
}
```

### 6. Multi-Tenancy

✅ **TenantId Everywhere**:
```csharp
// ✅ GOOD: TenantId set from service
var entity = Entity.Create(
    _currentTenant.TenantId,  // From ICurrentTenantService
    ...
);

// ❌ BAD: Hardcoded or missing TenantId
var entity = new Entity { TenantId = Guid.Empty };  // NEVER!
```

✅ **Global Query Filters**:
```csharp
// ✅ GOOD: Global filter in DbContext
modelBuilder.Entity<Invoice>()
    .HasQueryFilter(x => x.TenantId == _currentTenantService.TenantId);

// ❌ BAD: Manual filtering in every query
var invoices = await _context.Invoices
    .Where(x => x.TenantId == tenantId)  // Should be automatic!
    .ToListAsync();
```

### 7. Error Handling

✅ **Specific Exceptions**:
```csharp
// ✅ GOOD: Specific exceptions
if (invoice == null)
    throw new NotFoundException($"Invoice {id} not found");

if (invoice.Status == InvoiceStatus.Posted)
    throw new InvalidOperationException("Cannot modify posted invoices");

// ❌ BAD: Generic exceptions
if (invoice == null)
    throw new Exception("Error");  // Too generic
```

✅ **Exception Handling**:
```csharp
// ✅ GOOD: Handle at appropriate level
public async Task<IActionResult> Create(CreateInvoiceCommand command)
{
    try
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }
    catch (ValidationException ex)
    {
        return BadRequest(ex.Errors);
    }
    catch (NotFoundException ex)
    {
        return NotFound(ex.Message);
    }
}

// ❌ BAD: Swallow exceptions
try
{
    await _repository.SaveAsync(entity);
}
catch
{
    // Silent failure - BAD!
}
```

✅ **Logging**:
```csharp
// ✅ GOOD: Structured logging
try
{
    await ProcessInvoice(invoiceId);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to process invoice {InvoiceId}", invoiceId);
    throw;
}

// ❌ BAD: No logging
try
{
    await ProcessInvoice(invoiceId);
}
catch
{
    throw;  // No context logged
}
```

### 8. Testing

✅ **Unit Test Coverage**:
- All domain methods have tests
- Edge cases covered
- Validation tested
- State transitions tested

✅ **Test Naming**:
```csharp
// ✅ GOOD: Descriptive test names
[Fact]
public void Post_DraftInvoice_ShouldUpdateStatusAndSetPostedDate()

// ❌ BAD: Unclear test names
[Fact]
public void Test1()
```

✅ **AAA Pattern**:
```csharp
[Fact]
public void ApplyPayment_FullPayment_ShouldMarkInvoiceAsPaid()
{
    // Arrange
    var invoice = CreateTestInvoice(totalAmount: 100m);
    invoice.Post();

    // Act
    invoice.ApplyPayment(new Money(100m, "USD"));

    // Assert
    invoice.Status.Should().Be(InvoiceStatus.Paid);
    invoice.AmountPaid.Amount.Should().Be(100m);
}
```

### 9. Code Quality

✅ **Naming Conventions**:
- PascalCase for classes, methods, properties
- camelCase for parameters, local variables
- _camelCase for private fields
- UPPER_CASE for constants

✅ **Method Length**:
- Keep methods short and focused
- Extract complex logic to separate methods
- Aim for < 20 lines per method

✅ **Comments**:
```csharp
// ✅ GOOD: XML comments for public API
/// <summary>
/// Applies a payment to the invoice.
/// </summary>
/// <param name="payment">Payment amount</param>
/// <exception cref="InvalidOperationException">If invoice not posted</exception>
public void ApplyPayment(Money payment)

// ✅ GOOD: Explain why, not what
// Round to 2 decimals to prevent floating point errors
var total = Math.Round(sum, 2);

// ❌ BAD: Obvious comments
// Set status to posted
Status = InvoiceStatus.Posted;
```

✅ **Magic Numbers**:
```csharp
// ✅ GOOD: Named constants
private const int MaxLineItems = 100;
private const decimal TaxRate = 0.08m;

// ❌ BAD: Magic numbers
if (_items.Count > 100) { ... }
var tax = amount * 0.08m;
```

### 10. Financial Accuracy (for financial modules)

✅ **Decimal Types**:
```csharp
// ✅ GOOD: decimal for money
public decimal Amount { get; private set; }

// ❌ BAD: float or double for money
public double Amount { get; private set; }  // NEVER!
```

✅ **Double-Entry Bookkeeping**:
```csharp
// ✅ GOOD: Validate balanced entries
public void Post()
{
    if (TotalDebits != TotalCredits)
        throw new InvalidOperationException("Entry not balanced");
}
```

## Review Report Format

```
## Code Review Report

### File: [FileName.cs]
### Reviewed: [Date]
### Overall: ✅ PASS / ⚠️ NEEDS WORK / ❌ FAIL

---

### ✅ Strengths
- Good use of DDD patterns with aggregate roots
- Proper encapsulation with private setters
- Domain events for state changes
- Comprehensive unit tests

---

### ⚠️ Issues Found

#### Security
- **Line 45**: Missing [Authorize] attribute on controller action
  ```csharp
  // Change this:
  [HttpGet]
  public async Task<IActionResult> GetAll()

  // To this:
  [Authorize]
  [HttpGet]
  public async Task<IActionResult> GetAll()
  ```

#### Performance
- **Line 123**: Potential N+1 query
  ```csharp
  // Change this:
  var invoices = await _context.Invoices.ToListAsync();
  foreach (var invoice in invoices)
  {
      invoice.Client = await _context.Clients.FindAsync(invoice.ClientId);
  }

  // To this:
  var invoices = await _context.Invoices
      .Include(x => x.Client)
      .ToListAsync();
  ```

#### Code Quality
- **Line 67**: Method too long (35 lines), consider extracting
- **Line 89**: Magic number 0.08m should be a named constant

---

### 📋 Recommendations
1. Add [Authorize] attribute to all controller actions
2. Fix N+1 query in GetAllInvoices method
3. Extract validation logic from CreateInvoice to separate method
4. Add XML documentation to public methods
5. Increase test coverage for edge cases

---

### 📊 Metrics
- Methods reviewed: 15
- Issues found: 8 (3 security, 2 performance, 3 quality)
- Test coverage: 75% (target: 80%+)
```

## Best Practices Summary

- ✅ Follow SOLID principles
- ✅ Use DDD patterns (aggregates, value objects, domain events)
- ✅ Validate input at multiple layers
- ✅ Check authorization on all endpoints
- ✅ Prevent N+1 queries with Include/projections
- ✅ Use async/await for I/O operations
- ✅ Write comprehensive tests
- ✅ Use structured logging
- ✅ Handle exceptions appropriately
- ✅ Verify multi-tenancy isolation
- ✅ Use decimal for money (never float/double)
- ✅ Keep methods short and focused
- ✅ Document public APIs with XML comments
