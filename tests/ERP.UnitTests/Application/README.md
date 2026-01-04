# Application Layer Unit Tests

## Overview

This directory contains unit tests for the Application layer, specifically for command and query handlers. Unit tests use mocks to isolate the handler logic and verify business rules without requiring a database or external dependencies.

## Test Structure

### Command Handler Tests

Unit tests for command handlers verify:
- Business logic execution
- Validation rules
- Error handling and exceptions
- Multi-tenancy enforcement
- Authentication checks
- Dependency interactions (via mocks)

### Using Moq for Mocking

All tests use the Moq framework to create mock dependencies:

```csharp
private readonly Mock<IProjectRepository> _projectRepositoryMock;
private readonly Mock<IUnitOfWork> _unitOfWorkMock;
private readonly Mock<ICurrentTenantService> _currentTenantMock;
private readonly Mock<ICurrentUserService> _currentUserMock;
```

### Test Setup Pattern

Each test class follows this pattern:

```csharp
public class CreateProjectCommandHandlerTests
{
    // Mock dependencies
    private readonly Mock<IProjectRepository> _repositoryMock;
    private readonly CreateProjectCommandHandler _handler;
    private readonly Guid _testTenantId = Guid.NewGuid();

    public CreateProjectCommandHandlerTests()
    {
        // Create mocks
        _repositoryMock = new Mock<IProjectRepository>();

        // Setup common mock behaviors
        _currentTenantMock.Setup(x => x.TenantId).Returns(_testTenantId);

        // Create handler with mocked dependencies
        _handler = new CreateProjectCommandHandler(
            _repositoryMock.Object,
            ...
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new CreateProjectCommand { ... };
        _repositoryMock.Setup(x => x.GetByIdAsync(...)).ReturnsAsync(...);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        _repositoryMock.Verify(x => x.AddAsync(...), Times.Once);
    }
}
```

## Test Categories

### 1. Happy Path Tests

Verify successful execution with valid input:

```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldCreateProject()
{
    // Arrange
    var command = new CreateProjectCommand { Name = "Test" };

    // Act
    var result = await _handler.Handle(command);

    // Assert
    result.Should().BeGreaterThan(0);
    _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Project>()), Times.Once);
}
```

### 2. Error Scenario Tests

Verify proper exception handling:

```csharp
[Fact]
public async Task Handle_ProjectNotFound_ShouldThrowNotFoundException()
{
    // Arrange
    _repositoryMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Project?)null);

    // Act
    Func<Task> act = async () => await _handler.Handle(command);

    // Assert
    await act.Should().ThrowAsync<NotFoundException>()
        .WithMessage("*Project*");
}
```

### 3. Authentication Tests

Verify authorization checks:

```csharp
[Fact]
public async Task Handle_UnauthenticatedUser_ShouldThrowUnauthorizedException()
{
    // Arrange
    _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);

    // Act
    Func<Task> act = async () => await _handler.Handle(command);

    // Assert
    await act.Should().ThrowAsync<UnauthorizedException>();
}
```

### 4. Multi-Tenancy Tests

Verify tenant isolation:

```csharp
[Fact]
public async Task Handle_ClientBelongsToAnotherTenant_ShouldThrowNotFoundException()
{
    // Arrange
    var otherTenantId = Guid.NewGuid();
    var client = CreateClientForTenant(otherTenantId);

    // Act
    Func<Task> act = async () => await _handler.Handle(command);

    // Assert
    await act.Should().ThrowAsync<NotFoundException>();
}
```

### 5. Business Logic Tests

Verify complex business rules:

```csharp
[Fact]
public async Task Handle_MultipleLineItems_ShouldCalculateTotalCorrectly()
{
    // Arrange
    var command = CreateCommandWithLineItems(1500m, 2500m, 500m);

    // Act
    var result = await _handler.Handle(command);

    // Assert
    _invoiceRepositoryMock.Verify(
        x => x.AddAsync(It.Is<Invoice>(inv =>
            inv.TotalAmount.Amount == 4500.00m
        )),
        Times.Once
    );
}
```

## Mock Setup Patterns

### Setup Return Values

```csharp
_repositoryMock
    .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
    .ReturnsAsync(project);
```

### Setup Exceptions

```csharp
_repositoryMock
    .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
    .ThrowsAsync(new NotFoundException("Project not found"));
```

### Setup Sequences (for retry logic)

```csharp
_repositoryMock
    .SetupSequence(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(true)   // First call returns true
    .ReturnsAsync(false); // Second call returns false
```

### Verify Method Calls

```csharp
// Verify method was called once
_repositoryMock.Verify(
    x => x.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
    Times.Once
);

// Verify method was never called
_repositoryMock.Verify(
    x => x.DeleteAsync(It.IsAny<Project>()),
    Times.Never
);

// Verify with specific parameters
_repositoryMock.Verify(
    x => x.AddAsync(It.Is<Project>(p =>
        p.Name == "Expected Name" &&
        p.TenantId == _testTenantId
    ), It.IsAny<CancellationToken>()),
    Times.Once
);
```

## FluentAssertions Patterns

### Basic Assertions

```csharp
result.Should().BeGreaterThan(0);
result.Should().NotBeNull();
project.Name.Should().Be("Expected Name");
```

### Collection Assertions

```csharp
items.Should().HaveCount(5);
items.Should().Contain(x => x.Id == 1);
items.Should().NotContain(x => x.IsDeleted);
```

### Exception Assertions

```csharp
await act.Should().ThrowAsync<NotFoundException>();
await act.Should().ThrowAsync<ValidationException>()
    .WithMessage("*required*");
```

### Numeric Assertions

```csharp
total.Should().Be(1500.00m);
total.Should().BeApproximately(1500.00m, 0.01m);
```

## Running Tests

### Run all unit tests

```bash
cd tests/ERP.UnitTests
dotnet test
```

### Run specific test class

```bash
dotnet test --filter "FullyQualifiedName~CreateProjectCommandHandlerTests"
```

### Run with detailed output

```bash
dotnet test --verbosity detailed
```

### Run with code coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Best Practices

1. **Isolate Tests**: Each test should be independent and not rely on other tests
2. **Use Descriptive Names**: Test names should describe what they test
3. **Follow AAA Pattern**: Arrange, Act, Assert
4. **Mock External Dependencies**: Use mocks for repositories, external services
5. **Test Edge Cases**: Null values, empty collections, boundary conditions
6. **Test Business Rules**: Verify domain logic is enforced
7. **Test Security**: Verify authentication and authorization
8. **Test Multi-Tenancy**: Verify tenant isolation
9. **Verify Mock Interactions**: Use `Verify()` to ensure methods were called correctly
10. **Test Financial Accuracy**: For ERP - verify calculations are precise

## Common Patterns

### Test with Multiple Scenarios

```csharp
[Theory]
[InlineData(ProjectType.Billable, true)]
[InlineData(ProjectType.Overhead, false)]
[InlineData(ProjectType.Proposal, false)]
public async Task Handle_DifferentProjectTypes_ShouldSetBillableCorrectly(
    ProjectType type,
    bool expectedBillable)
{
    // Arrange, Act, Assert
}
```

### Helper Methods

Create helper methods to reduce test setup code:

```csharp
private Client CreateTestClient(Guid? tenantId = null)
{
    return Client.Create(
        tenantId ?? _testTenantId,
        "Test Client",
        ClientType.Corporate,
        null, null,
        Email.Create("test@example.com"),
        null, null
    );
}
```

## Test Coverage Goals

- **Critical Business Logic**: 100% coverage
- **Financial Calculations**: 100% coverage
- **Security/Authorization**: 100% coverage
- **Error Handling**: 100% coverage
- **Happy Paths**: 100% coverage
- **Overall Application Layer**: 80%+ coverage

## Troubleshooting

### Mock Setup Issues

If a mock isn't returning expected values:
- Check that `Setup()` parameters match the actual call
- Use `It.IsAny<T>()` for parameters you don't care about
- Use `It.Is<T>(predicate)` for specific matching

### Verification Failures

If `Verify()` fails:
- Check that the method was actually called
- Verify parameter matching is correct
- Use `VerifyAll()` to see all verification failures

### Async Test Issues

Always use `async Task` for test methods:
```csharp
[Fact]
public async Task TestMethod() // Correct
{
    await _handler.Handle(command);
}
```

Not:
```csharp
[Fact]
public void TestMethod() // Wrong - can cause timing issues
{
    _handler.Handle(command).Wait();
}
```
