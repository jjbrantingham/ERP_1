# Performance Review Skill

## Purpose
Analyze code for performance issues and suggest optimizations specific to the ERP SaaS application.

## Common Performance Issues

### 1. N+1 Query Problem

**The Problem:**
```csharp
// BAD - N+1 queries (1 + N additional queries)
var projects = await _context.Projects.ToListAsync();
foreach (var project in projects)
{
    // This executes a separate query for EACH project
    var client = await _context.Clients.FindAsync(project.ClientId);
    var wbsItems = await _context.WBSItems.Where(w => w.ProjectId == project.Id).ToListAsync();
}
```

**The Fix:**
```csharp
// GOOD - Single query with eager loading
var projects = await _context.Projects
    .Include(p => p.Client)
    .Include(p => p.WBSItems)
    .ToListAsync();
```

### 2. Loading Entire Tables

**The Problem:**
```csharp
// BAD - Loads ALL projects into memory
var projects = await _context.Projects.ToListAsync();
var activeProjects = projects.Where(p => p.Status == ProjectStatus.Active).ToList();
```

**The Fix:**
```csharp
// GOOD - Filter in database
var activeProjects = await _context.Projects
    .Where(p => p.Status == ProjectStatus.Active)
    .ToListAsync();
```

### 3. Missing Pagination

**The Problem:**
```csharp
// BAD - Returns ALL invoices (could be millions)
[HttpGet]
public async Task<IActionResult> GetInvoices()
{
    var invoices = await _context.Invoices.ToListAsync();
    return Ok(invoices);
}
```

**The Fix:**
```csharp
// GOOD - Paginated results
[HttpGet]
public async Task<IActionResult> GetInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
{
    var query = _context.Invoices.AsQueryable();
    var totalCount = await query.CountAsync();

    var invoices = await query
        .OrderByDescending(i => i.InvoiceDate)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return Ok(new PagedResult<Invoice>(invoices, totalCount, page, pageSize));
}
```

### 4. Over-fetching Data

**The Problem:**
```csharp
// BAD - Fetches all columns when only need a few
var invoices = await _context.Invoices
    .Include(i => i.Lines)
    .Include(i => i.Project)
    .Include(i => i.Client)
    .ToListAsync();

return invoices.Select(i => new { i.InvoiceNumber, i.TotalAmount }).ToList();
```

**The Fix:**
```csharp
// GOOD - Project to DTO in database
var invoices = await _context.Invoices
    .Select(i => new InvoiceSummaryDto
    {
        InvoiceNumber = i.InvoiceNumber,
        TotalAmount = i.TotalAmount
    })
    .ToListAsync();
```

### 5. Inefficient Aggregations

**The Problem:**
```csharp
// BAD - Loads all data then aggregates in memory
var entries = await _context.TimesheetEntries
    .Where(e => e.ProjectId == projectId)
    .ToListAsync();

var totalHours = entries.Sum(e => e.Hours);
```

**The Fix:**
```csharp
// GOOD - Aggregate in database
var totalHours = await _context.TimesheetEntries
    .Where(e => e.ProjectId == projectId)
    .SumAsync(e => e.Hours);
```

### 6. Synchronous I/O Operations

**The Problem:**
```csharp
// BAD - Blocking calls
public IActionResult GetInvoice(long id)
{
    var invoice = _context.Invoices.Find(id); // Blocking
    return Ok(invoice);
}
```

**The Fix:**
```csharp
// GOOD - Async all the way
public async Task<IActionResult> GetInvoice(long id)
{
    var invoice = await _context.Invoices.FindAsync(id);
    return Ok(invoice);
}
```

### 7. Missing Indexes

**Check for:**
- Foreign key columns without indexes
- Frequently queried columns without indexes
- Missing composite indexes for common query patterns

```sql
-- Add indexes for common queries
CREATE INDEX IX_Invoices_TenantId_InvoiceDate
ON bill.Invoices(TenantId, InvoiceDate DESC);

CREATE INDEX IX_TimesheetEntries_ProjectId_WorkDate
ON te.TimesheetEntries(ProjectId, WorkDate);

CREATE INDEX IX_Employees_TenantId_Status
ON hr.Employees(TenantId, Status)
WHERE Status = 1; -- Filtered index for active employees
```

### 8. Unnecessary Tracking

**The Problem:**
```csharp
// BAD - Tracking enabled for read-only queries
var invoices = await _context.Invoices
    .Include(i => i.Lines)
    .ToListAsync(); // Change tracking overhead

return invoices; // Just reading data
```

**The Fix:**
```csharp
// GOOD - Disable tracking for read-only queries
var invoices = await _context.Invoices
    .Include(i => i.Lines)
    .AsNoTracking()
    .ToListAsync();

return invoices;
```

### 9. Inefficient String Operations

**The Problem:**
```csharp
// BAD - String concatenation in loop
var report = "";
foreach (var line in lines)
{
    report += line + "\n"; // Creates new string each iteration
}
```

**The Fix:**
```csharp
// GOOD - Use StringBuilder
var report = new StringBuilder();
foreach (var line in lines)
{
    report.AppendLine(line);
}
return report.ToString();
```

### 10. No Caching

**The Problem:**
```csharp
// BAD - Hits database every time
public async Task<List<AccountType>> GetAccountTypesAsync()
{
    return await _context.AccountTypes.ToListAsync();
}
```

**The Fix:**
```csharp
// GOOD - Cache static/rarely changing data
private static List<AccountType> _accountTypesCache;
private static DateTime _cacheExpiration;

public async Task<List<AccountType>> GetAccountTypesAsync()
{
    if (_accountTypesCache == null || DateTime.UtcNow > _cacheExpiration)
    {
        _accountTypesCache = await _context.AccountTypes.ToListAsync();
        _cacheExpiration = DateTime.UtcNow.AddHours(1);
    }
    return _accountTypesCache;
}

// BETTER - Use IMemoryCache
public async Task<List<AccountType>> GetAccountTypesAsync()
{
    return await _cache.GetOrCreateAsync("AccountTypes", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        return await _context.AccountTypes.ToListAsync();
    });
}
```

## Performance Review Checklist

### Database Performance
- [ ] No N+1 queries (use Include/ThenInclude)
- [ ] Filters applied in database (not in memory)
- [ ] Projections used (Select to DTO)
- [ ] Pagination implemented for lists
- [ ] Aggregations done in database
- [ ] AsNoTracking() for read-only queries
- [ ] Appropriate indexes exist
- [ ] No SELECT N+1 (batch queries where possible)

### API Performance
- [ ] Async/await used consistently
- [ ] Appropriate HTTP caching headers
- [ ] Response compression enabled
- [ ] Minimal data returned (DTOs, not entities)
- [ ] No over-fetching
- [ ] Batch operations where possible

### Memory Performance
- [ ] Large collections streamed, not loaded entirely
- [ ] StringBuilder for string concatenation
- [ ] Dispose patterns implemented
- [ ] No memory leaks (event handlers unsubscribed)

### Caching Strategy
- [ ] Static data cached (account types, tax rates)
- [ ] Expensive computations cached
- [ ] Cache invalidation strategy
- [ ] Distributed cache for scale-out scenarios

## Performance Testing Tools

### EF Core Query Logging
```csharp
// In appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Application Insights
```csharp
// Track operation performance
using (var operation = _telemetryClient.StartOperation<DependencyTelemetry>("GenerateInvoice"))
{
    // Your code
    operation.Telemetry.Success = true;
}
```

### MiniProfiler
```csharp
// In Program.cs
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/profiler";
}).AddEntityFramework();

// In controller
public async Task<IActionResult> GetInvoices()
{
    using (MiniProfiler.Current.Step("Get Invoices"))
    {
        return Ok(await _mediator.Send(new GetInvoicesQuery()));
    }
}
```

## Performance Goals

### API Response Times
- **List endpoints**: < 500ms
- **Detail endpoints**: < 200ms
- **Create/Update**: < 1000ms
- **Reports**: < 5000ms (or use background jobs)

### Database Query Times
- **Simple queries**: < 50ms
- **Complex queries**: < 200ms
- **Report queries**: < 1000ms

### Page Load Times
- **First Contentful Paint**: < 1.5s
- **Time to Interactive**: < 3.5s
- **Largest Contentful Paint**: < 2.5s

## Optimization Strategies

### 1. Use Compiled Queries
```csharp
private static readonly Func<ERPDbContext, long, Task<Project>> _getProjectById =
    EF.CompileAsyncQuery((ERPDbContext context, long id) =>
        context.Projects
            .Include(p => p.Client)
            .Include(p => p.WBSItems)
            .FirstOrDefault(p => p.Id == id));

public async Task<Project> GetProjectByIdAsync(long id)
{
    return await _getProjectById(_context, id);
}
```

### 2. Batch Database Operations
```csharp
// BAD - Multiple round trips
foreach (var entry in entries)
{
    await _context.TimesheetEntries.AddAsync(entry);
    await _context.SaveChangesAsync();
}

// GOOD - Single round trip
await _context.TimesheetEntries.AddRangeAsync(entries);
await _context.SaveChangesAsync();
```

### 3. Use Background Jobs for Long-Running Tasks
```csharp
// Don't block API response for long operations
[HttpPost("generate-monthly-invoices")]
public IActionResult GenerateMonthlyInvoices()
{
    _backgroundJobClient.Enqueue(() => GenerateInvoicesAsync());
    return Accepted();
}
```

## Performance Review Report Format

### 1. Summary
- Overall performance assessment
- Critical issues count
- Average response times

### 2. Critical Performance Issues
Issues causing significant performance degradation.

### 3. Optimization Opportunities
Quick wins for performance improvements.

### 4. Database Query Analysis
Slow queries with optimization suggestions.

### 5. Caching Recommendations
What should be cached and cache strategy.

### 6. Index Recommendations
Missing indexes that would improve performance.

## Example Usage

```
User: /performance-review "Invoice generation process"

Claude: I'll analyze the invoice generation process for performance issues...

CRITICAL ISSUES:
1. N+1 Query in InvoiceService.GenerateInvoice()
   - Loading timesheet entries one at a time
   - Impact: 500ms per invoice → 50s for 100 invoices
   - Fix: Use Include() to eager load

RECOMMENDATIONS:
1. Add index on TimesheetEntries(ProjectId, IsBillable)
2. Cache billing rates (rarely change)
3. Use compiled query for project lookup

[Complete analysis with code examples]
```

## Related Skills
- optimize-queries
- code-review
- database-schema-review
