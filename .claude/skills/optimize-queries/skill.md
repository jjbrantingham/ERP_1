# Optimize Queries Skill

## Purpose
Analyze and optimize EF Core database queries for performance.

## Optimization Techniques

### 1. Use Include for Eager Loading
```csharp
// BAD
var invoices = await _context.Invoices.ToListAsync();
foreach (var invoice in invoices)
{
    var client = await _context.Clients.FindAsync(invoice.ClientId);
}

// GOOD
var invoices = await _context.Invoices
    .Include(i => i.Client)
    .ToListAsync();
```

### 2. Use AsNoTracking for Read-Only
```csharp
var reports = await _context.Projects
    .AsNoTracking()
    .ToListAsync();
```

### 3. Project to DTO in Database
```csharp
var dtos = await _context.Invoices
    .Select(i => new InvoiceDto
    {
        Id = i.Id,
        InvoiceNumber = i.InvoiceNumber,
        TotalAmount = i.TotalAmount
    })
    .ToListAsync();
```

### 4. Add Indexes
```sql
CREATE INDEX IX_Invoices_TenantId_InvoiceDate
ON bill.Invoices(TenantId, InvoiceDate DESC);
```

## Related Skills
- performance-review
- database-schema-review
