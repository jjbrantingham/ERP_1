# Generate Integration Tests Skill

## Purpose
Generate integration tests for repositories, API endpoints, and database operations using WebApplicationFactory and test database.

## Integration Test Setup

```csharp
public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ERPDbContext DbContext;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        DbContext = CreateDbContext();
        SeedTestData();
    }

    protected ERPDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ERPDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        return new ERPDbContext(options);
    }

    protected virtual void SeedTestData()
    {
        // Add common test data
    }

    public void Dispose()
    {
        DbContext?.Dispose();
        Client?.Dispose();
    }
}
```

## Testing Repositories

```csharp
public class InvoiceRepositoryTests : IntegrationTestBase
{
    private readonly IInvoiceRepository _repository;

    public InvoiceRepositoryTests(WebApplicationFactory<Program> factory) : base(factory)
    {
        _repository = new InvoiceRepository(DbContext);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingInvoice_ReturnsInvoice()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        DbContext.Invoices.Add(invoice);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(invoice.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(invoice.Id);
    }

    [Fact]
    public async Task AddAsync_NewInvoice_AddsToDatabase()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        await _repository.AddAsync(invoice);
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.Invoices.FindAsync(invoice.Id);
        saved.Should().NotBeNull();
    }
}
```

## Testing API Endpoints

```csharp
public class InvoicesControllerTests : IntegrationTestBase
{
    public InvoicesControllerTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_ValidInvoice_ReturnsCreated()
    {
        // Arrange
        var command = new CreateInvoiceCommand
        {
            ProjectId = 1,
            ClientId = 1,
            InvoiceDate = DateTime.UtcNow
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoices", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var invoiceId = await response.Content.ReadFromJsonAsync<long>();
        invoiceId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetById_ExistingInvoice_ReturnsOk()
    {
        // Arrange
        var invoiceId = await CreateTestInvoiceViaApi();

        // Act
        var response = await Client.GetAsync($"/api/v1/invoices/{invoiceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invoice = await response.Content.ReadFromJsonAsync<InvoiceDto>();
        invoice.Should().NotBeNull();
        invoice.Id.Should().Be(invoiceId);
    }
}
```

## Testing Multi-Tenancy Isolation

```csharp
[Fact]
public async Task GetInvoices_DifferentTenant_ReturnsOnlyOwnInvoices()
{
    // Arrange
    var tenant1 = Guid.NewGuid();
    var tenant2 = Guid.NewGuid();

    DbContext.Invoices.Add(CreateInvoice(tenant1));
    DbContext.Invoices.Add(CreateInvoice(tenant2));
    await DbContext.SaveChangesAsync();

    // Act - Query as tenant1
    var results = await _repository.GetByTenantAsync(tenant1);

    // Assert
    results.Should().HaveCount(1);
    results.All(i => i.TenantId == tenant1).Should().BeTrue();
}
```

## Related Skills
- generate-unit-tests
- verify-tenant-isolation
