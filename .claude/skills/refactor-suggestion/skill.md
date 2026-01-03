# Refactor Suggestion Skill

## Purpose
Identify code smells and suggest refactoring opportunities to improve code quality, maintainability, and adherence to SOLID principles.

## Code Smells to Identify

### 1. Long Method
**Smell:** Method with too many lines (> 50 lines) or responsibilities.

**Refactor:** Extract methods

```csharp
// BEFORE
public async Task<Invoice> GenerateInvoice(long projectId, DateTime startDate, DateTime endDate)
{
    // 100+ lines of code doing validation, calculations, data fetching, etc.
}

// AFTER
public async Task<Invoice> GenerateInvoice(long projectId, DateTime startDate, DateTime endDate)
{
    await ValidateInputsAsync(projectId, startDate, endDate);
    var project = await GetProjectWithDataAsync(projectId);
    var timesheets = await GetBillableTimesheetsAsync(projectId, startDate, endDate);
    var expenses = await GetBillableExpensesAsync(projectId, startDate, endDate);
    var invoice = CreateInvoice(project, timesheets, expenses);
    await SaveInvoiceAsync(invoice);
    return invoice;
}
```

### 2. Large Class
**Smell:** Class with too many responsibilities (> 300 lines or many methods).

**Refactor:** Split into multiple classes following Single Responsibility Principle.

```csharp
// BEFORE - God class
public class InvoiceService
{
    // Invoice generation
    // Payment processing
    // Email sending
    // PDF generation
    // Tax calculations
    // All in one class!
}

// AFTER - Separated responsibilities
public class InvoiceGenerationService { }
public class PaymentProcessingService { }
public class InvoiceEmailService { }
public class InvoicePdfGenerator { }
public class TaxCalculationService { }
```

### 3. Duplicate Code
**Smell:** Same or similar code in multiple places.

**Refactor:** Extract to common method or base class.

```csharp
// BEFORE - Duplicated validation
public class CreateProjectCommand
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ValidationException("Name is required");
        if (Name.Length > 200)
            throw new ValidationException("Name too long");
    }
}

public class UpdateProjectCommand
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ValidationException("Name is required");
        if (Name.Length > 200)
            throw new ValidationException("Name too long");
    }
}

// AFTER - Shared validator
public class ProjectNameValidator : AbstractValidator<string>
{
    public ProjectNameValidator()
    {
        RuleFor(name => name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name too long");
    }
}
```

### 4. Long Parameter List
**Smell:** Method with too many parameters (> 4).

**Refactor:** Introduce parameter object.

```csharp
// BEFORE
public void CreateInvoice(
    long projectId,
    long clientId,
    DateTime invoiceDate,
    DateTime dueDate,
    string description,
    decimal taxRate,
    bool includeExpenses,
    bool includeMilestones)
{
    // ...
}

// AFTER
public class InvoiceCreationParams
{
    public long ProjectId { get; set; }
    public long ClientId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Description { get; set; }
    public decimal TaxRate { get; set; }
    public bool IncludeExpenses { get; set; }
    public bool IncludeMilestones { get; set; }
}

public void CreateInvoice(InvoiceCreationParams params)
{
    // ...
}
```

### 5. Feature Envy
**Smell:** Method that uses data/methods from another class more than its own.

**Refactor:** Move method to the class it's envious of.

```csharp
// BEFORE - InvoiceService envying Invoice class data
public class InvoiceService
{
    public decimal CalculateTotal(Invoice invoice)
    {
        var subtotal = invoice.Lines.Sum(l => l.Quantity * l.UnitPrice);
        var tax = subtotal * invoice.TaxRate;
        return subtotal + tax;
    }
}

// AFTER - Move calculation to Invoice
public class Invoice
{
    public decimal CalculateTotal()
    {
        var subtotal = Lines.Sum(l => l.Quantity * l.UnitPrice);
        var tax = subtotal * TaxRate;
        return subtotal + tax;
    }
}
```

### 6. Primitive Obsession
**Smell:** Using primitives instead of small objects for simple tasks.

**Refactor:** Replace with value objects.

```csharp
// BEFORE - Primitives everywhere
public class Invoice
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public string Email { get; set; }
}

// AFTER - Value objects
public class Invoice
{
    public Money Amount { get; set; }
    public Email ClientEmail { get; set; }
}

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    // Validation, equality, operations
}

public class Email : ValueObject
{
    public string Value { get; }
    // Email validation
}
```

### 7. Switch Statements
**Smell:** Large switch/if-else chains based on type.

**Refactor:** Replace with polymorphism.

```csharp
// BEFORE
public decimal CalculateBillingAmount(Project project, Timesheet timesheet)
{
    switch (project.BillingMode)
    {
        case BillingMode.TimeAndMaterial:
            return timesheet.TotalHours * project.HourlyRate;
        case BillingMode.FixedPrice:
            return project.FixedPrice / project.EstimatedHours * timesheet.TotalHours;
        case BillingMode.Milestone:
            return project.CurrentMilestone.Amount;
        default:
            throw new NotSupportedException();
    }
}

// AFTER - Strategy pattern
public interface IBillingStrategy
{
    decimal CalculateAmount(Project project, Timesheet timesheet);
}

public class TimeAndMaterialBilling : IBillingStrategy { }
public class FixedPriceBilling : IBillingStrategy { }
public class MilestoneBilling : IBillingStrategy { }

public class Project
{
    private IBillingStrategy _billingStrategy;

    public decimal CalculateBillingAmount(Timesheet timesheet)
    {
        return _billingStrategy.CalculateAmount(this, timesheet);
    }
}
```

### 8. Data Class
**Smell:** Class with only properties, no behavior.

**Refactor:** Move behavior from other classes into the data class.

```csharp
// BEFORE - Anemic domain model
public class Invoice
{
    public InvoiceStatus Status { get; set; }
    public decimal Amount { get; set; }
}

public class InvoiceService
{
    public void PostInvoice(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.Draft)
            throw new InvalidOperationException();
        invoice.Status = InvoiceStatus.Posted;
    }
}

// AFTER - Rich domain model
public class Invoice
{
    public InvoiceStatus Status { get; private set; }
    public decimal Amount { get; private set; }

    public void Post()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be posted");

        Status = InvoiceStatus.Posted;
        AddDomainEvent(new InvoicePostedEvent(Id));
    }
}
```

### 9. Comments
**Smell:** Excessive comments explaining what code does.

**Refactor:** Make code self-explanatory.

```csharp
// BEFORE
// Get all active projects for the current tenant
// and filter by client if clientId is provided
// then order by creation date descending
var projects = await _context.Projects
    .Where(p => p.TenantId == tenantId && p.Status == 1)
    .Where(p => clientId == null || p.ClientId == clientId)
    .OrderByDescending(p => p.CreatedDate)
    .ToListAsync();

// AFTER - Self-explanatory
var projects = await GetActiveProjects Async(tenantId, clientId);

private async Task<List<Project>> GetActiveProjectsAsync(Guid tenantId, long? clientId = null)
{
    var query = _context.Projects
        .Where(p => p.TenantId == tenantId)
        .Where(p => p.Status == ProjectStatus.Active);

    if (clientId.HasValue)
        query = query.Where(p => p.ClientId == clientId.Value);

    return await query
        .OrderByDescending(p => p.CreatedDate)
        .ToListAsync();
}
```

### 10. Dead Code
**Smell:** Unused methods, properties, classes.

**Refactor:** Delete it. (Version control is your backup.)

## SOLID Principles Violations

### Single Responsibility Principle (SRP)
```csharp
// VIOLATION - Multiple responsibilities
public class InvoiceService
{
    public Invoice Generate() { }
    public void SendEmail() { }
    public byte[] GeneratePdf() { }
    public void ProcessPayment() { }
}

// FIXED - One responsibility per class
public class InvoiceGenerationService { }
public class InvoiceEmailService { }
public class InvoicePdfGenerator { }
public class PaymentProcessor { }
```

### Open/Closed Principle (OCP)
```csharp
// VIOLATION - Need to modify class to add new types
public class ReportGenerator
{
    public void Generate(ReportType type)
    {
        if (type == ReportType.ProfitLoss) { }
        else if (type == ReportType.BalanceSheet) { }
        // Need to modify to add new report type
    }
}

// FIXED - Open for extension, closed for modification
public interface IReport
{
    void Generate();
}

public class ProfitLossReport : IReport { }
public class BalanceSheetReport : IReport { }
// Add new reports without modifying existing code
```

### Liskov Substitution Principle (LSP)
```csharp
// VIOLATION - Derived class changes behavior unexpectedly
public class Project
{
    public virtual void Close()
    {
        Status = ProjectStatus.Closed;
    }
}

public class FixedPriceProject : Project
{
    public override void Close()
    {
        if (PercentComplete < 100)
            throw new Exception("Cannot close until 100% complete");
        base.Close();
    }
}

// FIXED - Consistent behavior
public abstract class Project
{
    public void Close()
    {
        if (!CanClose())
            throw new InvalidOperationException(GetClosureRequirement());

        Status = ProjectStatus.Closed;
    }

    protected abstract bool CanClose();
    protected abstract string GetClosureRequirement();
}
```

### Interface Segregation Principle (ISP)
```csharp
// VIOLATION - Fat interface
public interface IRepository
{
    Task<T> GetByIdAsync(long id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(long id);
    Task<PagedResult<T>> GetPagedAsync(int page, int size);
    Task<int> CountAsync();
    Task BulkInsertAsync(List<T> entities);
    // Many more methods...
}

// FIXED - Segregated interfaces
public interface IReadRepository<T>
{
    Task<T> GetByIdAsync(long id);
    Task<PagedResult<T>> GetPagedAsync(int page, int size);
}

public interface IWriteRepository<T>
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(long id);
}
```

### Dependency Inversion Principle (DIP)
```csharp
// VIOLATION - Depends on concrete implementation
public class InvoiceService
{
    private readonly SqlServerInvoiceRepository _repository;

    public InvoiceService()
    {
        _repository = new SqlServerInvoiceRepository();
    }
}

// FIXED - Depends on abstraction
public class InvoiceService
{
    private readonly IInvoiceRepository _repository;

    public InvoiceService(IInvoiceRepository repository)
    {
        _repository = repository;
    }
}
```

## Refactoring Checklist

- [ ] Methods are < 50 lines
- [ ] Classes have single responsibility
- [ ] No duplicate code
- [ ] Parameter lists are reasonable (< 4 parameters)
- [ ] Code is self-explanatory (minimal comments)
- [ ] SOLID principles followed
- [ ] Domain logic in domain layer
- [ ] No primitive obsession (use value objects)
- [ ] Appropriate use of design patterns
- [ ] No dead code

## Example Usage

```
User: /refactor-suggestion "ProjectsController.cs"

Claude: I'll analyze ProjectsController for refactoring opportunities...

CODE SMELLS FOUND:

1. Long Method: GetProjectDetails() (87 lines)
   Suggested refactoring:
   - Extract method for loading related data
   - Extract method for authorization check
   - Extract method for DTO mapping

2. Feature Envy: CalculateProjectCost() in ProjectService
   - Calculation uses only Project data
   - Should move to Project aggregate

[Complete analysis with code examples]
```

## Related Skills
- code-review
- architecture-review
- domain-model-design
