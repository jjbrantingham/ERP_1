# ERP Integration Tests

## Overview

This project contains integration tests for the ERP application. Integration tests verify that components work correctly together, including command handlers, repositories, database interactions, and cross-cutting concerns like authentication and multi-tenancy.

## Test Infrastructure

### IntegrationTestWebAppFactory

Custom `WebApplicationFactory` that configures the test environment:
- Uses in-memory database for testing
- Configures test-specific services
- Enables sensitive data logging for debugging

### IntegrationTestBase

Base class for integration tests providing:
- Access to `DbContext` and `UnitOfWork`
- Service resolution via `GetService<T>()`
- Database cleanup utilities
- Automatic disposal of test resources

### TestAuthenticationHelper

Helper for creating test authentication contexts:
- Mock `ICurrentUserService` for authenticated/unauthenticated scenarios
- Mock `ICurrentTenantService` for multi-tenancy testing
- Predefined test tenant and user IDs

## Test Categories

### 1. Command Handler Tests

Tests for individual command handlers verifying:
- Successful command execution
- Database persistence
- Business rule enforcement
- Error scenarios (not found, validation failures)
- Unique constraint validation

**Examples:**
- `CreateProjectCommandHandlerTests` - Project creation and validation
- `CreateInvoiceCommandHandlerTests` - Invoice creation and financial calculations

### 2. Authorization Tests

Tests verifying authentication and authorization:
- Unauthenticated user rejection
- Missing authentication claims
- Handler-level security checks

**Examples:**
- `AuthorizationTests` - Authorization enforcement across handlers

### 3. Multi-Tenancy Isolation Tests

CRITICAL tests for SaaS data isolation:
- Tenant-specific data filtering
- Prevention of cross-tenant data access
- Automatic tenant ID assignment
- Global query filter verification

**Examples:**
- `MultiTenancyIsolationTests` - Tenant isolation verification

## Running Tests

### Run All Integration Tests

```bash
cd tests/ERP.IntegrationTests
dotnet test
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~CreateProjectCommandHandlerTests"
```

### Run with Detailed Output

```bash
dotnet test --verbosity detailed
```

### Run with Code Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Writing New Integration Tests

### 1. Create Test Class

```csharp
public class MyCommandHandlerTests : IntegrationTestBase
{
    private readonly IMediator _mediator;

    public MyCommandHandlerTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        _mediator = GetService<IMediator>();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new MyCommand { /* ... */ };

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
    }
}
```

### 2. Use FluentAssertions

```csharp
result.Should().BeGreaterThan(0);
invoice.TotalAmount!.Amount.Should().Be(1500.00m);
await act.Should().ThrowAsync<NotFoundException>();
```

### 3. Clean Up Test Data

The base class handles disposal automatically, but you can manually clear data:

```csharp
await ClearDatabaseAsync();
```

## Test Data Helpers

Create helper methods in your test classes to set up test data:

```csharp
private async Task<Client> CreateTestClientAsync()
{
    var client = Client.Create(
        TestAuthenticationHelper.TestTenantId,
        "Test Client",
        ClientType.Corporate,
        null, null, Email.Create("test@example.com"), null, null
    );

    DbContext.Set<Client>().Add(client);
    await DbContext.SaveChangesAsync();

    return client;
}
```

## Best Practices

1. **Isolate Tests**: Each test should be independent
2. **Use Realistic Data**: Test with data similar to production
3. **Test Edge Cases**: Null values, empty collections, boundary conditions
4. **Test Business Rules**: Verify domain logic is enforced
5. **Test Multi-Tenancy**: Always verify tenant isolation for SaaS
6. **Test Financial Accuracy**: Critical for ERP - verify calculations precisely
7. **Use Descriptive Names**: Test method names should describe what they test

## Test Naming Convention

```
[MethodUnderTest]_[Scenario]_[ExpectedBehavior]
```

Examples:
- `Handle_ValidCommand_ShouldCreateProject`
- `Handle_InvalidProject_ShouldThrowNotFoundException`
- `CreateInvoice_MultipleLineItems_ShouldCalculateTotalCorrectly`

## Common Assertions

```csharp
// Existence
result.Should().NotBeNull();
project.Should().BeNull();

// Equality
project.Name.Should().Be("Expected Name");
invoice.TotalAmount!.Amount.Should().Be(1500.00m);

// Collections
projects.Should().HaveCount(5);
projects.Should().Contain(p => p.Id == projectId);

// Exceptions
await act.Should().ThrowAsync<NotFoundException>();
await act.Should().ThrowAsync<ValidationException>()
    .WithMessage("*required*");

// Numeric comparisons
id.Should().BeGreaterThan(0);
amount.Should().BeLessThanOrEqualTo(1000);
```

## Troubleshooting

### In-Memory Database Limitations

The in-memory database has some limitations:
- No referential integrity enforcement
- No SQL Server-specific features
- Consider using SQL Server test containers for full integration

### Test Isolation Issues

If tests fail when run together but pass individually:
- Ensure proper cleanup between tests
- Check for shared static state
- Verify database is properly reset

### Authentication Context

If authorization tests fail:
- Verify `ICurrentUserService` is properly mocked
- Check that test user context is configured correctly
- Ensure authentication middleware is bypassed in tests
