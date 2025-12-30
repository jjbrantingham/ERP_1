# ERP SaaS Application - Claude Code Configuration

## Project Overview

This is an Enterprise Resource Planning (ERP) SaaS application designed for project-based businesses. The system manages projects, employees, clients, timesheets, expenses, financial accounting, and billing.

### Technology Stack
- **Backend**: ASP.NET Core 9.0, C# 13
- **Frontend**: Razor Pages/Blazor + Tailwind CSS
- **Database**: Azure SQL Server
- **ORM**: Entity Framework Core 9.0
- **Architecture**: Domain-Driven Design (DDD) with CQRS
- **Patterns**: Repository, Unit of Work, Specification
- **Cloud**: Microsoft Azure

---

## Project Structure

```
src/
├── ERP.Web/              # Presentation layer (Razor Pages/Blazor + Tailwind)
├── ERP.Application/      # Application layer (CQRS, Use Cases, DTOs)
├── ERP.Domain/           # Domain layer (Entities, Aggregates, Value Objects)
├── ERP.Infrastructure/   # Infrastructure layer (EF Core, Repositories, External Services)
└── ERP.Shared/           # Shared kernel (Common utilities, constants)

tests/
├── ERP.UnitTests/        # Unit tests for domain logic
├── ERP.IntegrationTests/ # Integration tests for repositories and APIs
└── ERP.FunctionalTests/  # End-to-end functional tests
```

---

## Bounded Contexts

The application is organized into the following bounded contexts:

1. **Project Management** (`pm` schema)
   - Projects (Billable, Overhead, Proposal types)
   - Work Breakdown Structure (WBS)
   - Contracts
   - Resource Allocations

2. **Human Resources** (`hr` schema)
   - Employees
   - Resource Types
   - Rates (Cost & Billing)

3. **Customer Relationship Management** (`crm` schema)
   - Clients
   - Contacts
   - Notes

4. **Vendor Management** (`vm` schema)
   - Contractors
   - Vendors

5. **Time & Expense** (`te` schema)
   - Timesheets
   - Expense Reports
   - Approval Workflows

6. **Financial Management** (`fin` schema)
   - Chart of Accounts
   - General Ledger
   - Accounts Receivable
   - Accounts Payable
   - Double-entry bookkeeping

7. **Billing** (`bill` schema)
   - Invoices
   - Multiple billing modes (T&M, Fixed Price, % Complete, Milestone)
   - Payments

8. **Reporting** (`rpt` schema)
   - Financial Reports
   - Project Reports
   - Operational Reports
   - Dashboards

---

## Coding Standards & Best Practices

### Domain Layer

#### Aggregate Design
- **Aggregate Root**: Must inherit from `AggregateRoot` base class
- **Entities**: Must inherit from `Entity` base class
- **Value Objects**: Must inherit from `ValueObject` base class
- **Encapsulation**: All setters must be private; use methods for state changes
- **Invariants**: Must be enforced in the aggregate root
- **Domain Events**: Use for significant state changes

**Example Aggregate Root**:
```csharp
public class Project : AggregateRoot
{
    public ProjectId Id { get; private set; }
    public string Name { get; private set; }
    public ProjectStatus Status { get; private set; }

    private readonly List<WorkBreakdownStructureItem> _wbsItems;
    public IReadOnlyCollection<WorkBreakdownStructureItem> WBSItems => _wbsItems.AsReadOnly();

    // Methods, not property setters
    public void ChangeStatus(ProjectStatus newStatus)
    {
        // Validate state transition
        if (!IsValidStatusTransition(Status, newStatus))
            throw new InvalidOperationException($"Cannot change status from {Status} to {newStatus}");

        var oldStatus = Status;
        Status = newStatus;

        // Raise domain event
        AddDomainEvent(new ProjectStatusChangedEvent(Id, oldStatus, newStatus));
    }

    private bool IsValidStatusTransition(ProjectStatus from, ProjectStatus to)
    {
        // Business rules for state transitions
        return true; // Implement logic
    }
}
```

#### Value Objects
- Must be immutable
- Equality based on all properties
- No identity
- Use for concepts like Money, DateRange, Email, Address

**Example Value Object**:
```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
}
```

#### Domain Events
- Use for significant state changes
- Name in past tense (e.g., `ProjectCreatedEvent`)
- Include all relevant data
- Keep serializable

**Example Domain Event**:
```csharp
public class ProjectStatusChangedEvent : DomainEvent
{
    public ProjectId ProjectId { get; }
    public ProjectStatus OldStatus { get; }
    public ProjectStatus NewStatus { get; }
    public DateTime ChangedAt { get; }

    public ProjectStatusChangedEvent(ProjectId projectId, ProjectStatus oldStatus, ProjectStatus newStatus)
    {
        ProjectId = projectId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = DateTime.UtcNow;
    }
}
```

### Application Layer

#### CQRS Pattern
- **Commands**: Change state, return void or ID
- **Queries**: Read data, return DTOs
- **Handlers**: One handler per command/query
- **Validation**: Use FluentValidation
- **MediatR**: For command/query dispatch

**Example Command**:
```csharp
public class CreateProjectCommand : IRequest<long>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ProjectType Type { get; set; }
    public long ClientId { get; set; }
}

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.ClientId).GreaterThan(0);
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, long>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // Verify client exists and belongs to tenant
        var client = await _clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client == null || client.TenantId != _currentTenant.TenantId)
            throw new NotFoundException("Client not found");

        // Generate project number
        var projectNumber = await GenerateProjectNumberAsync(cancellationToken);

        // Create project
        var project = Project.Create(
            _currentTenant.TenantId,
            projectNumber,
            request.Name,
            request.Description,
            request.Type,
            request.ClientId
        );

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.Id;
    }

    private async Task<string> GenerateProjectNumberAsync(CancellationToken cancellationToken)
    {
        // Implementation
        return "PRJ-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
    }
}
```

**Example Query**:
```csharp
public class GetProjectByIdQuery : IRequest<ProjectDto>
{
    public long ProjectId { get; set; }
}

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
            throw new NotFoundException("Project not found");

        return _mapper.Map<ProjectDto>(project);
    }
}
```

### Infrastructure Layer

#### Repository Pattern
- One repository per aggregate root
- Return domain entities, not EF entities
- Implement in Infrastructure, interface in Domain

**Example Repository**:
```csharp
public interface IProjectRepository : IRepository<Project>
{
    Task<Project> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Project> GetByProjectNumberAsync(string projectNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Project>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<Project> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.WBSItems)
            .Include(p => p.ResourceAllocations)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Project> GetByProjectNumberAsync(string projectNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.ProjectNumber == projectNumber, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.Status == ProjectStatus.Active)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Project>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Projects.AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Project>(items, totalCount, page, pageSize);
    }
}
```

#### EF Core Configuration
- Use Fluent API, not data annotations
- Configure in separate configuration classes
- One configuration class per entity

**Example Entity Configuration**:
```csharp
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", "pm");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("ProjectId")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.ProjectNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<byte>();

        // Value object mapping
        builder.OwnsOne(p => p.Budget, budget =>
        {
            budget.Property(m => m.Amount)
                .HasColumnName("BudgetAmount")
                .HasPrecision(18, 2);
            budget.Property(m => m.Currency)
                .HasColumnName("BudgetCurrency")
                .HasMaxLength(3);
        });

        // Collections
        builder.HasMany(p => p.WBSItems)
            .WithOne()
            .HasForeignKey("ProjectId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => new { p.TenantId, p.ProjectNumber })
            .IsUnique();
        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.Status);

        // Global query filter for multi-tenancy
        builder.HasQueryFilter(p => p.TenantId == CurrentTenantId);

        // Row version for optimistic concurrency
        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        // Audit fields
        builder.Property(p => p.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
```

### Multi-Tenancy

#### Critical Requirements
- **ALWAYS** filter by `TenantId` in queries
- **ALWAYS** set `TenantId` when creating entities
- Use global query filters in EF Core
- Verify tenant isolation in tests

**Example Tenant Service**:
```csharp
public interface ICurrentTenantService
{
    Guid TenantId { get; }
    string TenantName { get; }
}

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId");
            if (tenantIdClaim == null)
                throw new InvalidOperationException("TenantId not found in claims");
            return Guid.Parse(tenantIdClaim.Value);
        }
    }

    public string TenantName => _httpContextAccessor.HttpContext?.User?.FindFirst("TenantName")?.Value;
}
```

### API Controllers

#### Standards
- Inherit from `ApiControllerBase`
- Use `[ApiController]` attribute
- Use proper HTTP verbs
- Return appropriate status codes
- Add `[Authorize]` for secured endpoints
- Use `[ProducesResponseType]` for Swagger

**Example Controller**:
```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProjectsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var query = new GetProjectByIdQuery { ProjectId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create new project
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProjectCommand command)
    {
        var projectId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = projectId }, projectId);
    }

    /// <summary>
    /// Update project
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProjectCommand command)
    {
        command.ProjectId = id;
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Delete project
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var command = new DeleteProjectCommand { ProjectId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Search projects with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SearchProjectsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

### Testing

#### Unit Tests
- Test domain logic
- Test business rules
- Test validation
- Use AAA pattern (Arrange, Act, Assert)
- Use FluentAssertions
- Mock dependencies with Moq

**Example Unit Test**:
```csharp
public class ProjectTests
{
    [Fact]
    public void ChangeStatus_ValidTransition_ShouldUpdateStatus()
    {
        // Arrange
        var project = CreateTestProject();
        var newStatus = ProjectStatus.OnHold;

        // Act
        project.ChangeStatus(newStatus);

        // Assert
        project.Status.Should().Be(newStatus);
        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectStatusChangedEvent>();
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ShouldThrowException()
    {
        // Arrange
        var project = CreateTestProject();
        project.ChangeStatus(ProjectStatus.Completed);

        // Act & Assert
        project.Invoking(p => p.ChangeStatus(ProjectStatus.Active))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot change status*");
    }

    private Project CreateTestProject()
    {
        return Project.Create(
            Guid.NewGuid(),
            "PRJ-001",
            "Test Project",
            "Description",
            ProjectType.Billable,
            1
        );
    }
}
```

#### Integration Tests
- Test repositories
- Test API endpoints
- Use WebApplicationFactory
- Use in-memory database or test database
- Test multi-tenancy isolation

**Example Integration Test**:
```csharp
public class ProjectsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProjectsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ValidProject_ReturnsCreated()
    {
        // Arrange
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "Test Description",
            Type = ProjectType.Billable,
            ClientId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/projects", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var projectId = await response.Content.ReadFromJsonAsync<long>();
        projectId.Should().BeGreaterThan(0);
    }
}
```

### Tailwind CSS

#### Usage Guidelines
- Use utility classes
- Follow mobile-first approach
- Use responsive prefixes (sm:, md:, lg:, xl:)
- Create custom components in `@layer components`
- Use semantic color names from theme

**Example UI Component**:
```html
<div class="bg-white shadow-md rounded-lg p-6 mb-4">
    <h2 class="text-2xl font-bold text-gray-800 mb-4">Projects</h2>

    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        @foreach (var project in Model.Projects)
        {
            <div class="border border-gray-200 rounded-lg p-4 hover:shadow-lg transition-shadow">
                <h3 class="text-lg font-semibold text-gray-900 mb-2">@project.Name</h3>
                <p class="text-sm text-gray-600 mb-2">@project.Description</p>

                <div class="flex justify-between items-center">
                    <span class="px-3 py-1 rounded-full text-xs font-medium
                        @(project.Status == ProjectStatus.Active ? "bg-green-100 text-green-800" :
                          project.Status == ProjectStatus.OnHold ? "bg-yellow-100 text-yellow-800" :
                          "bg-gray-100 text-gray-800")">
                        @project.Status
                    </span>

                    <a href="/projects/@project.Id" class="text-blue-600 hover:text-blue-800 text-sm font-medium">
                        View Details →
                    </a>
                </div>
            </div>
        }
    </div>
</div>
```

---

## Important Reminders

### Security
- ✅ ALWAYS validate user input
- ✅ ALWAYS check authorization
- ✅ ALWAYS verify tenant isolation
- ✅ NEVER trust client input
- ✅ NEVER log sensitive data (passwords, SSN, credit cards)
- ✅ ALWAYS use parameterized queries (EF Core does this)
- ✅ ALWAYS sanitize HTML output

### Performance
- ✅ ALWAYS use async/await for I/O operations
- ✅ ALWAYS use pagination for lists
- ✅ AVOID N+1 queries (use Include/ThenInclude)
- ✅ USE projections for queries (Select to DTO)
- ✅ CONSIDER caching for frequently accessed data
- ✅ USE database indexes appropriately

### Financial Accuracy
- ✅ ALWAYS use `decimal` for money amounts
- ✅ ALWAYS round financial calculations properly
- ✅ ALWAYS validate double-entry bookkeeping (debits = credits)
- ✅ ALWAYS maintain audit trail for financial transactions
- ✅ NEVER delete financial records (soft delete only)

### Error Handling
- ✅ USE custom exceptions for domain errors
- ✅ HANDLE exceptions at appropriate levels
- ✅ LOG exceptions with context
- ✅ RETURN meaningful error messages to users
- ✅ NEVER expose stack traces to end users

---

## Common Tasks

### Adding a New Aggregate

1. Create domain entities in `ERP.Domain/[BoundedContext]/Entities/`
2. Create value objects in `ERP.Domain/[BoundedContext]/ValueObjects/`
3. Create repository interface in `ERP.Domain/[BoundedContext]/Repositories/`
4. Create repository implementation in `ERP.Infrastructure/Persistence/Repositories/`
5. Create EF configuration in `ERP.Infrastructure/Persistence/Configurations/`
6. Create and apply migration
7. Create commands/queries in `ERP.Application/[BoundedContext]/`
8. Create DTOs in `ERP.Application/[BoundedContext]/DTOs/`
9. Create controller in `ERP.Web/Controllers/`
10. Create UI pages in `ERP.Web/Pages/`
11. Write unit tests
12. Write integration tests

### Creating a New Migration

```bash
# From ERP.Infrastructure directory
dotnet ef migrations add MigrationName --startup-project ../ERP.Web

# Apply migration
dotnet ef database update --startup-project ../ERP.Web

# Review migration before applying to production!
```

### Running Tests

```bash
# All tests
dotnet test

# Specific test project
dotnet test tests/ERP.UnitTests

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## File Naming Conventions

- **Entities**: PascalCase (e.g., `Project.cs`, `Invoice.cs`)
- **Interfaces**: IPascalCase (e.g., `IProjectRepository.cs`)
- **Commands**: `VerbNounCommand.cs` (e.g., `CreateProjectCommand.cs`)
- **Queries**: `VerbNounQuery.cs` (e.g., `GetProjectByIdQuery.cs`)
- **Handlers**: `CommandNameHandler.cs` (e.g., `CreateProjectCommandHandler.cs`)
- **DTOs**: `NounDto.cs` (e.g., `ProjectDto.cs`)
- **Validators**: `CommandNameValidator.cs` (e.g., `CreateProjectCommandValidator.cs`)
- **Tests**: `ClassNameTests.cs` (e.g., `ProjectTests.cs`)

---

## Git Workflow

### Branch Naming
- `feature/feature-name` - New features
- `bugfix/bug-description` - Bug fixes
- `hotfix/urgent-fix` - Production hotfixes
- `refactor/what-is-refactored` - Refactoring

### Commit Messages
Follow Conventional Commits:
- `feat: Add invoice generation`
- `fix: Correct timesheet calculation`
- `refactor: Simplify project repository`
- `test: Add tests for billing module`
- `docs: Update API documentation`
- `chore: Update dependencies`

---

## Additional Resources

- **Architecture**: See `docs/ARCHITECTURE.md`
- **Domain Models**: See `docs/DOMAIN_MODEL.md`
- **Database Schema**: See `docs/DATABASE_SCHEMA.md`
- **Implementation Roadmap**: See `docs/IMPLEMENTATION_ROADMAP.md`
- **Skills**: See `docs/SKILLS_SUGGESTIONS.md`

---

## Questions to Ask

When implementing new features, consider:

1. **Does this belong in the domain layer or application layer?**
   - Business logic → Domain
   - Orchestration, validation, DTO mapping → Application

2. **Is multi-tenancy properly implemented?**
   - Is TenantId set on creation?
   - Is TenantId filtered in queries?
   - Are tests verifying tenant isolation?

3. **Is this secure?**
   - Is authorization checked?
   - Is input validated?
   - Are sensitive operations logged?

4. **Is this testable?**
   - Can it be unit tested?
   - Are dependencies injected?
   - Is it using interfaces?

5. **Will this perform well?**
   - Are queries optimized?
   - Is pagination used?
   - Are N+1 queries avoided?

6. **Is this maintainable?**
   - Is code clear and self-documenting?
   - Are responsibilities well-separated?
   - Is it following SOLID principles?

---

## When in Doubt

- Follow DDD principles
- Keep aggregates small
- Encapsulate business logic in the domain
- Use CQRS for clear separation
- Test thoroughly, especially financial logic
- Verify multi-tenancy isolation
- Prioritize security and data integrity

---

**Remember**: This is a financial system. Accuracy, security, and auditability are paramount. When in doubt, ask before proceeding with critical financial or security-related code.
