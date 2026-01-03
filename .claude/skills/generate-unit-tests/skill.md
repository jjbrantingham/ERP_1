# Generate Unit Tests Skill

## Purpose
Generate comprehensive unit tests for domain logic, aggregates, and business rules using xUnit, FluentAssertions, and Moq.

## Test Structure (AAA Pattern)

```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Setup
    var sut = CreateSystemUnderTest();
    var input = CreateTestInput();

    // Act - Execute
    var result = sut.MethodToTest(input);

    // Assert - Verify
    result.Should().Be(expectedValue);
}
```

## What to Test

### Domain Aggregates
- Creation (factory methods)
- Business rule enforcement
- Invariant validation
- State transitions
- Domain event generation

### Value Objects
- Validation
- Equality
- Immutability
- Operations

### Command Handlers
- Successful execution
- Validation failures
- Authorization checks
- Exception handling

### Query Handlers
- Data retrieval
- Filtering logic
- Projections

## Example Templates

### Testing Aggregate
```csharp
public class InvoiceTests
{
    [Fact]
    public void Create_ValidData_CreatesInvoice()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceNumber = "INV-001";

        // Act
        var invoice = Invoice.Create(tenantId, invoiceNumber, projectId, clientId);

        // Assert
        invoice.Should().NotBeNull();
        invoice.TenantId.Should().Be(tenantId);
        invoice.InvoiceNumber.Should().Be(invoiceNumber);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<InvoiceCreatedEvent>();
    }

    [Fact]
    public void Post_DraftInvoice_ChangesStatusToPosted()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        invoice.Post();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Posted);
        invoice.DomainEvents.Should().Contain(e => e is InvoicePostedEvent);
    }

    [Fact]
    public void Post_AlreadyPostedInvoice_ThrowsException()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.Post();

        // Act & Assert
        invoice.Invoking(i => i.Post())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*already posted*");
    }

    private Invoice CreateTestInvoice()
    {
        return Invoice.Create(Guid.NewGuid(), "INV-001", 1L, 1L);
    }
}
```

### Testing Command Handler
```csharp
public class CreateInvoiceCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentTenantService> _tenantServiceMock;
    private readonly CreateInvoiceCommandHandler _handler;

    public CreateInvoiceCommandHandlerTests()
    {
        _repositoryMock = new Mock<IInvoiceRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tenantServiceMock = new Mock<ICurrentTenantService>();

        _tenantServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        _handler = new CreateInvoiceCommandHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _tenantServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesInvoice()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            ProjectId = 1,
            ClientId = 1,
            InvoiceDate = DateTime.UtcNow
        };

        // Act
        var invoiceId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        invoiceId.Should().BeGreaterThan(0);
        _repositoryMock.Verify(x => x.AddAsync(
            It.Is<Invoice>(i => i.ProjectId == command.ProjectId),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### Testing Value Object
```csharp
public class MoneyTests
{
    [Fact]
    public void Constructor_ValidAmount_CreatesMoney()
    {
        // Act
        var money = new Money(100.50m, "USD");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_NegativeAmount_ThrowsException()
    {
        // Act & Assert
        Action act = () => new Money(-10, "USD");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(100, "USD");

        // Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(50, "USD");

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsException()
    {
        // Arrange
        var money1 = new Money(100, "USD");
        var money2 = new Money(50, "EUR");

        // Act & Assert
        money1.Invoking(m => m.Add(money2))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*different currencies*");
    }
}
```

## FluentAssertions Common Patterns

```csharp
// Collections
result.Should().BeEmpty();
result.Should().HaveCount(5);
result.Should().Contain(item);
result.Should().NotContain(item);
result.Should().ContainSingle(x => x.Id == 1);

// Nullability
result.Should().BeNull();
result.Should().NotBeNull();

// Booleans
result.Should().BeTrue();
result.Should().BeFalse();

// Numeric
result.Should().Be(42);
result.Should().BeGreaterThan(10);
result.Should().BeLessThan(100);
result.Should().BeInRange(1, 100);

// Strings
result.Should().Be("expected");
result.Should().Contain("substring");
result.Should().StartWith("prefix");
result.Should().EndWith("suffix");
result.Should().Match("*pattern*");

// Exceptions
act.Should().Throw<InvalidOperationException>();
act.Should().Throw<ArgumentException>()
    .WithMessage("*expected message*")
    .And.ParamName.Should().Be("paramName");

// Types
result.Should().BeOfType<Invoice>();
result.Should().BeAssignableTo<IEntity>();

// Dates
date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
date.Should().BeAfter(earlierDate);
date.Should().BeBefore(laterDate);
```

## Test Data Builders

```csharp
public class InvoiceBuilder
{
    private Guid _tenantId = Guid.NewGuid();
    private string _invoiceNumber = "INV-001";
    private long _projectId = 1;
    private long _clientId = 1;

    public InvoiceBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public InvoiceBuilder WithInvoiceNumber(string number)
    {
        _invoiceNumber = number;
        return this;
    }

    public Invoice Build()
    {
        return Invoice.Create(_tenantId, _invoiceNumber, _projectId, _clientId);
    }
}

// Usage
var invoice = new InvoiceBuilder()
    .WithInvoiceNumber("INV-123")
    .Build();
```

## Related Skills
- generate-integration-tests
- test-coverage-report
- code-review
