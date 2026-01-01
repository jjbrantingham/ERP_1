# Verify Multi-Tenancy Isolation

Comprehensive verification of multi-tenancy isolation to prevent data leakage between tenants.

## What This Skill Does

Verifies that multi-tenancy is properly implemented across all layers:

1. **Domain Layer**: TenantId property on all aggregates
2. **Repository Layer**: TenantId filtering in queries
3. **EF Core**: Global query filters configured
4. **Application Layer**: TenantId set on entity creation
5. **API Layer**: Tenant context properly resolved
6. **Tests**: Tenant isolation tests exist

## Verification Checklist

### 1. Domain Entities

Check each aggregate root in `src/ERP.Domain/`:

```csharp
✅ Has TenantId property:
   public Guid TenantId { get; private set; }

✅ TenantId is set in factory method:
   public static Entity Create(Guid tenantId, ...)
   {
       return new Entity
       {
           TenantId = tenantId,
           ...
       };
   }

✅ TenantId is required and indexed
```

### 2. EF Core Configurations

Check `src/ERP.Infrastructure/Persistence/Configurations/`:

```csharp
✅ TenantId property configured:
   builder.Property(x => x.TenantId)
       .IsRequired();

✅ TenantId in indexes:
   builder.HasIndex(x => x.TenantId);
   builder.HasIndex(x => new { x.TenantId, x.SomeUniqueField })
       .IsUnique();

✅ Global query filter (if applicable):
   // Note in configuration that this is set in DbContext
```

### 3. DbContext Global Filters

Check `src/ERP.Infrastructure/Persistence/ERPDbContext.cs`:

```csharp
✅ Global query filters defined in OnModelCreating:

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Apply configurations
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ERPDbContext).Assembly);

    // Global query filters for multi-tenancy
    var tenantId = _currentTenantService?.TenantId ?? Guid.Empty;

    // Example for each bounded context
    modelBuilder.Entity<Project>().HasQueryFilter(x => x.TenantId == tenantId);
    modelBuilder.Entity<Invoice>().HasQueryFilter(x => x.TenantId == tenantId);
    // ... etc for all tenant-scoped entities
}
```

### 4. Repository Implementations

Check `src/ERP.Infrastructure/Persistence/Repositories/`:

```csharp
✅ All queries automatically filter by TenantId (via global filters)
✅ Unique field queries include TenantId:

   // GOOD: Includes TenantId validation
   var project = await _context.Projects
       .FirstOrDefaultAsync(x => x.ProjectNumber == projectNumber, cancellationToken);
   // Global filter automatically adds: && x.TenantId == currentTenantId

✅ No IgnoreQueryFilters() used without explicit reason and documentation
```

### 5. Command Handlers

Check `src/ERP.Application/{Context}/Commands/`:

```csharp
✅ Creates entities with TenantId from ICurrentTenantService:

   public class CreateEntityCommandHandler
   {
       private readonly ICurrentTenantService _currentTenant;

       public async Task<long> Handle(CreateEntityCommand request, ...)
       {
           var entity = Entity.Create(
               _currentTenant.TenantId,  // ← Always from current tenant
               request.Name,
               ...
           );

           await _repository.AddAsync(entity, cancellationToken);
           await _unitOfWork.SaveChangesAsync(cancellationToken);

           return entity.Id;
       }
   }

✅ Update/Delete commands verify tenant ownership:

   var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
   if (entity == null)
       throw new NotFoundException(); // Global filter prevents cross-tenant access
```

### 6. Query Handlers

Check `src/ERP.Application/{Context}/Queries/`:

```csharp
✅ Queries automatically filtered by TenantId via global filters
✅ No raw SQL queries bypassing filters
✅ Projections maintain tenant isolation
```

### 7. API Controllers

Check `src/ERP.Web/Controllers/`:

```csharp
✅ All controllers have [Authorize] attribute
✅ Current tenant resolved from claims:

   public class InvoicesController : ApiControllerBase
   {
       // ICurrentTenantService injected and used in handlers
   }
```

### 8. CurrentTenantService

Check `src/ERP.Application/Common/Services/CurrentTenantService.cs`:

```csharp
✅ Properly extracts TenantId from claims:

   public class CurrentTenantService : ICurrentTenantService
   {
       private readonly IHttpContextAccessor _httpContextAccessor;

       public Guid TenantId
       {
           get
           {
               var tenantIdClaim = _httpContextAccessor.HttpContext?.User
                   ?.FindFirst("TenantId");

               if (tenantIdClaim == null)
                   throw new InvalidOperationException("TenantId not found in claims");

               return Guid.Parse(tenantIdClaim.Value);
           }
       }
   }
```

### 9. Authentication

Check `src/ERP.Web/Program.cs` or authentication configuration:

```csharp
✅ TenantId included in JWT claims during login
✅ TenantId validated in authentication middleware
```

### 10. Database Migrations

Check migration files in `src/ERP.Infrastructure/Persistence/Migrations/`:

```csharp
✅ All entity tables have TenantId column
✅ TenantId columns are NOT NULL
✅ Indexes include TenantId where appropriate
✅ Foreign keys don't bypass tenant boundaries
```

## Automated Tests

### Unit Tests

Create tests in `tests/ERP.UnitTests/MultiTenancy/`:

```csharp
public class TenantIsolationTests
{
    [Fact]
    public void CreateEntity_ShouldSetTenantIdFromService()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var mockTenantService = new Mock<ICurrentTenantService>();
        mockTenantService.Setup(x => x.TenantId).Returns(tenantId);

        // Act
        var entity = Entity.Create(tenantId, "Test");

        // Assert
        entity.TenantId.Should().Be(tenantId);
    }
}
```

### Integration Tests

Create tests in `tests/ERP.IntegrationTests/MultiTenancy/`:

```csharp
public class TenantIsolationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetEntity_FromDifferentTenant_ShouldReturnNotFound()
    {
        // Arrange: Create entity for Tenant A
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ERPDbContext>();

        var entity = Entity.Create(tenantA, "Test");
        context.Entities.Add(entity);
        await context.SaveChangesAsync();

        // Act: Try to access as Tenant B
        var client = _factory.CreateClientWithTenant(tenantB);
        var response = await client.GetAsync($"/api/entities/{entity.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListEntities_ShouldOnlyReturnCurrentTenantData()
    {
        // Arrange: Create entities for multiple tenants
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ERPDbContext>();

        context.Entities.Add(Entity.Create(tenantA, "Tenant A Entity"));
        context.Entities.Add(Entity.Create(tenantB, "Tenant B Entity"));
        await context.SaveChangesAsync();

        // Act: Query as Tenant A
        var client = _factory.CreateClientWithTenant(tenantA);
        var response = await client.GetAsync("/api/entities");
        var entities = await response.Content.ReadFromJsonAsync<List<EntityDto>>();

        // Assert
        entities.Should().HaveCount(1);
        entities.First().Name.Should().Be("Tenant A Entity");
    }
}
```

## Common Vulnerabilities

### ❌ DANGEROUS: Bypassing Query Filters

```csharp
// NEVER DO THIS without explicit reason and audit:
var entity = await _context.Entities
    .IgnoreQueryFilters()  // ← Bypasses tenant isolation!
    .FirstOrDefaultAsync(x => x.Id == id);
```

### ❌ DANGEROUS: Hardcoded TenantId

```csharp
// NEVER DO THIS:
var entity = Entity.Create(
    Guid.Parse("12345678-1234-1234-1234-123456789012"),  // ← Hardcoded!
    request.Name
);

// ALWAYS DO THIS:
var entity = Entity.Create(
    _currentTenant.TenantId,  // ← From service
    request.Name
);
```

### ❌ DANGEROUS: Raw SQL Without Tenant Filter

```csharp
// NEVER DO THIS:
var entities = await _context.Entities
    .FromSqlRaw("SELECT * FROM Entities WHERE Id = {0}", id)  // ← No tenant filter!
    .ToListAsync();

// ALWAYS DO THIS:
var entities = await _context.Entities
    .FromSqlRaw("SELECT * FROM Entities WHERE Id = {0} AND TenantId = {1}", id, tenantId)
    .ToListAsync();
```

### ❌ DANGEROUS: Missing TenantId in Unique Constraints

```csharp
// WRONG: Unique across all tenants
builder.HasIndex(x => x.ProjectNumber)
    .IsUnique();

// CORRECT: Unique per tenant
builder.HasIndex(x => new { x.TenantId, x.ProjectNumber })
    .IsUnique();
```

## Security Audit Commands

Run these to verify tenant isolation:

```bash
# 1. Check all entities have TenantId property
grep -r "public class.*: AggregateRoot" src/ERP.Domain/ | while read line; do
    file=$(echo $line | cut -d: -f1)
    if ! grep -q "TenantId" "$file"; then
        echo "MISSING TenantId: $file"
    fi
done

# 2. Check all configurations set TenantId as required
grep -r "IEntityTypeConfiguration" src/ERP.Infrastructure/Persistence/Configurations/ | while read line; do
    file=$(echo $line | cut -d: -f1)
    if ! grep -q "TenantId.*IsRequired" "$file"; then
        echo "WARNING: TenantId not required in $file"
    fi
done

# 3. Find IgnoreQueryFilters usage (review each)
grep -r "IgnoreQueryFilters" src/

# 4. Find raw SQL (review each for tenant filter)
grep -r "FromSqlRaw\|ExecuteSqlRaw" src/
```

## Remediation Steps

If tenant isolation issues are found:

1. **Add TenantId to Entity**:
   - Add property to domain entity
   - Update EF Core configuration
   - Create migration to add column
   - Add index on TenantId

2. **Add Global Query Filter**:
   - Add filter in DbContext.OnModelCreating
   - Test that queries are filtered

3. **Update Tests**:
   - Add tenant isolation tests
   - Verify cross-tenant access is blocked

4. **Security Review**:
   - Review all IgnoreQueryFilters calls
   - Review all raw SQL queries
   - Review unique constraints include TenantId

## Report Format

Provide a report with:

```
## Tenant Isolation Verification Report

### Summary
- ✅ X entities verified
- ⚠️ Y warnings found
- ❌ Z critical issues found

### Details

#### ✅ Compliant Entities
- Project (PM module)
- Invoice (Billing module)
...

#### ⚠️ Warnings
- Employee entity: TenantId not in unique index for EmployeeNumber

#### ❌ Critical Issues
- [None found / List issues]

### Recommendations
1. Add TenantId to EmployeeNumber unique index
2. Add integration tests for cross-tenant access prevention
3. Review raw SQL in ReportRepository.cs

### Test Coverage
- Unit tests: X% coverage
- Integration tests: Y tenant isolation tests
- Missing tests for: [Module names]
```

## Best Practices

- ✅ **ALWAYS** use ICurrentTenantService for TenantId
- ✅ **ALWAYS** include TenantId in unique constraints
- ✅ **ALWAYS** configure global query filters
- ✅ **ALWAYS** test tenant isolation
- ✅ **NEVER** use IgnoreQueryFilters without security review
- ✅ **NEVER** hardcode TenantId values
- ✅ **NEVER** use raw SQL without tenant filtering
