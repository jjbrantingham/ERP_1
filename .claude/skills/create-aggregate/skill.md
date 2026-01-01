# Create DDD Aggregate

Generate a complete Domain-Driven Design aggregate for the ERP system following established patterns.

## What This Skill Does

Creates a complete aggregate with all necessary files and patterns:

1. **Domain Layer**:
   - Aggregate root entity class
   - Related entity classes (if needed)
   - Value objects (if needed)
   - Domain events
   - Repository interface
   - Enum types

2. **Infrastructure Layer**:
   - Repository implementation
   - EF Core entity configurations
   - Database migration

3. **Application Layer**:
   - DTOs (Data Transfer Objects)
   - AutoMapper profile
   - Basic CQRS commands and queries

4. **Tests**:
   - Unit tests for aggregate

## Expected Input

Ask the user for:
- **Aggregate name** (e.g., "Invoice", "Subscription", "Payment")
- **Bounded context** (e.g., "Billing", "Financial", "Reporting")
- **Schema prefix** (e.g., "bill", "fin", "rpt")
- **Key properties** and their types
- **Business rules** and invariants
- **Related entities** (if any)
- **Value objects** needed (if any)

## Implementation Guidelines

### 1. Domain Entity

Create in `src/ERP.Domain/{Context}/Entities/{AggregateName}.cs`:

```csharp
using ERP.Domain.Common.Entities;
using ERP.Domain.{Context}.ValueObjects;
using ERP.Domain.{Context}.Enums;
using ERP.Domain.{Context}.Events;

namespace ERP.Domain.{Context}.Entities;

/// <summary>
/// {Description of aggregate}
/// </summary>
public class {AggregateName} : AggregateRoot
{
    public long Id { get; private set; }
    public Guid TenantId { get; private set; }

    // Properties with private setters
    public string PropertyName { get; private set; }
    public {EnumType} Status { get; private set; }

    // Collections as readonly
    private readonly List<{ChildEntity}> _items;
    public IReadOnlyCollection<{ChildEntity}> Items => _items.AsReadOnly();

    // Audit fields
    public DateTime CreatedDate { get; private set; }
    public DateTime? ModifiedDate { get; private set; }
    public byte[] RowVersion { get; private set; }

    // Private constructor for EF Core
    private {AggregateName}()
    {
        _items = new List<{ChildEntity}>();
    }

    // Factory method
    public static {AggregateName} Create(
        Guid tenantId,
        // parameters
    )
    {
        var entity = new {AggregateName}
        {
            TenantId = tenantId,
            CreatedDate = DateTime.UtcNow,
            Status = {DefaultStatus}
        };

        // Raise domain event
        entity.AddDomainEvent(new {AggregateName}CreatedEvent(entity.Id, tenantId));

        return entity;
    }

    // Business logic methods (not property setters!)
    public void ChangeStatus({EnumType} newStatus)
    {
        // Validate state transition
        if (!IsValidStatusTransition(Status, newStatus))
            throw new InvalidOperationException($"Cannot change status from {Status} to {newStatus}");

        var oldStatus = Status;
        Status = newStatus;
        ModifiedDate = DateTime.UtcNow;

        AddDomainEvent(new {AggregateName}StatusChangedEvent(Id, TenantId, oldStatus, newStatus));
    }

    private bool IsValidStatusTransition({EnumType} from, {EnumType} to)
    {
        // Implement business rules
        return true;
    }
}
```

### 2. Enums

Create in `src/ERP.Domain/{Context}/Enums/{EnumName}.cs`:

```csharp
namespace ERP.Domain.{Context}.Enums;

public enum {EnumName} : byte
{
    Value1 = 1,
    Value2 = 2,
    // etc.
}
```

### 3. Domain Events

Create in `src/ERP.Domain/{Context}/Events/{EventName}.cs`:

```csharp
using ERP.Domain.Common.Events;

namespace ERP.Domain.{Context}.Events;

public class {AggregateName}CreatedEvent : DomainEvent
{
    public long {AggregateName}Id { get; }
    public Guid TenantId { get; }

    public {AggregateName}CreatedEvent(long id, Guid tenantId)
    {
        {AggregateName}Id = id;
        TenantId = tenantId;
    }
}
```

### 4. Repository Interface

Create in `src/ERP.Domain/{Context}/Repositories/I{AggregateName}Repository.cs`:

```csharp
using ERP.Domain.Common.Interfaces;
using ERP.Domain.{Context}.Entities;

namespace ERP.Domain.{Context}.Repositories;

public interface I{AggregateName}Repository : IRepository<{AggregateName}>
{
    Task<{AggregateName}?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<{AggregateName}>> GetAllAsync(CancellationToken cancellationToken = default);
    // Add specific query methods
}
```

### 5. Repository Implementation

Create in `src/ERP.Infrastructure/Persistence/Repositories/{AggregateName}Repository.cs`:

```csharp
using ERP.Domain.{Context}.Entities;
using ERP.Domain.{Context}.Repositories;
using ERP.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

public class {AggregateName}Repository : Repository<{AggregateName}>, I{AggregateName}Repository
{
    public {AggregateName}Repository(ERPDbContext context) : base(context)
    {
    }

    public async Task<{AggregateName}?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.{PluralName}
            .Include(x => x.Items)  // Include child collections
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<{AggregateName}>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.{PluralName}
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
```

### 6. EF Core Configuration

Create in `src/ERP.Infrastructure/Persistence/Configurations/{AggregateName}Configuration.cs`:

```csharp
using ERP.Domain.{Context}.Entities;
using ERP.Domain.{Context}.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class {AggregateName}Configuration : IEntityTypeConfiguration<{AggregateName}>
{
    public void Configure(EntityTypeBuilder<{AggregateName}> builder)
    {
        builder.ToTable("{PluralName}", "{schema}");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("{AggregateName}Id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<byte>();

        // Configure other properties

        // Audit fields
        builder.Property(x => x.CreatedDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.ModifiedDate);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey("{AggregateName}Id")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.Status);
    }
}
```

### 7. DTOs

Create in `src/ERP.Application/{Context}/DTOs/{AggregateName}Dto.cs`:

```csharp
namespace ERP.Application.{Context}.DTOs;

public class {AggregateName}Dto
{
    public long Id { get; set; }
    public string PropertyName { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
```

### 8. Migration

After creating all files:

1. Update `ERPDbContext.cs` to add DbSet
2. Run: `dotnet ef migrations add Add{Context}Module --startup-project ../ERP.Web`
3. Review the migration
4. Update database: `dotnet ef database update --startup-project ../ERP.Web`

### 9. Register in DI

Update `src/ERP.Web/Program.cs`:

```csharp
// Add repository
builder.Services.AddScoped<I{AggregateName}Repository, {AggregateName}Repository>();
```

## Multi-Tenancy Requirements

✅ **ALWAYS**:
- Include `TenantId` property
- Filter by `TenantId` in all queries
- Set `TenantId` when creating entities
- Add `TenantId` to indexes

## Validation Checklist

After generation, verify:

- [ ] Aggregate has private setters
- [ ] Business logic in methods, not properties
- [ ] Domain events for significant state changes
- [ ] Factory method for creation
- [ ] Repository interface in Domain
- [ ] Repository implementation in Infrastructure
- [ ] EF Core configuration with indexes
- [ ] Multi-tenancy properly implemented
- [ ] Optimistic concurrency with RowVersion
- [ ] Audit fields (CreatedDate, ModifiedDate)
- [ ] DTOs for queries
- [ ] Migration created and reviewed

## Example Usage

```
User: "Create an Invoice aggregate in the Billing context"

Assistant response:
1. Ask for Invoice properties (InvoiceNumber, DueDate, TotalAmount, etc.)
2. Ask for Invoice statuses (Draft, Sent, Paid, Overdue, Cancelled, etc.)
3. Ask for child entities (InvoiceLineItem?)
4. Ask for value objects (Money, InvoiceNumber?)
5. Ask for business rules (e.g., "Can only send draft invoices")
6. Generate all files following the patterns above
7. Create migration
8. Register in DI
```

## Best Practices

- Keep aggregates small and focused
- Enforce invariants in the aggregate root
- Use value objects for concepts without identity
- Raise domain events for significant state changes
- Use factory methods instead of public constructors
- Always validate state transitions
- Include comprehensive XML documentation
