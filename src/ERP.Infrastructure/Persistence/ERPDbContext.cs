using System.Reflection;
using ERP.Application.Common.Interfaces;
using ERP.Domain.Common;
using ERP.Domain.Common.Entities;
using ERP.Domain.CRM.Entities;
using ERP.Domain.HR.Entities;
using ERP.Domain.Identity.Entities;
using ERP.Domain.PM.Entities;
using ERP.Domain.TE.Entities;
using ERP.Domain.VM.Entities;
using ERP.Domain.FIN.Entities;
using ERP.Domain.BILL.Entities;
using ERP.Domain.WF.Entities;
using ERP.Domain.AUDIT.Entities;
using ERP.Domain.AUDIT.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERP.Infrastructure.Persistence;

/// <summary>
/// Main database context for the ERP application.
/// Implements multi-tenancy with global query filters.
/// </summary>
public class ERPDbContext : DbContext, IDbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMediator _mediator;

    public ERPDbContext(
        DbContextOptions<ERPDbContext> options,
        ICurrentTenantService currentTenantService,
        ICurrentUserService currentUserService,
        IMediator mediator)
        : base(options)
    {
        _currentTenantService = currentTenantService;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    // Common
    public DbSet<Tenant> Tenants => Set<Tenant>();

    // Identity (identity schema)
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Project Management (pm schema)
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<WBSItem> WBSItems => Set<WBSItem>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ResourceAllocation> ResourceAllocations => Set<ResourceAllocation>();

    // Human Resources (hr schema)
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<ResourceType> ResourceTypes => Set<ResourceType>();
    public DbSet<Rate> Rates => Set<Rate>();

    // Customer Relationship (crm schema)
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Note> Notes => Set<Note>();

    // Vendor Management (vm schema)
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<VendorContact> VendorContacts => Set<VendorContact>();
    public DbSet<VendorNote> VendorNotes => Set<VendorNote>();

    // Time & Expense (te schema)
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
    public DbSet<ExpenseReport> ExpenseReports => Set<ExpenseReport>();
    public DbSet<ExpenseItem> ExpenseItems => Set<ExpenseItem>();

    // Financial Management (fin schema)
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();

    // Billing (bill schema)
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Workflow (wf schema)
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<StepInstance> StepInstances => Set<StepInstance>();

    // Audit (audit schema)
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply global query filter for multi-tenancy
        ApplyGlobalFilters(modelBuilder);
    }

    /// <summary>
    /// Applies global query filters for multi-tenancy.
    /// All entities inheriting from Entity will automatically be filtered by TenantId.
    /// </summary>
    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var type = entityType.ClrType;

            // Skip Tenant entity itself from the filter
            if (type == typeof(Tenant))
                continue;

            // Only apply filter to entities that inherit from Entity
            if (typeof(Entity).IsAssignableFrom(type))
            {
                var method = typeof(ERPDbContext)
                    .GetMethod(nameof(SetGlobalQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)?
                    .MakeGenericMethod(type);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    /// <summary>
    /// Sets a global query filter for an entity type to filter by TenantId.
    /// Filter is only applied when a tenant is set; otherwise, no filtering occurs.
    /// </summary>
    private void SetGlobalQueryFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : Entity
    {
        // Use a filter that checks IsSet first to avoid throwing when no tenant is configured
        // When tenant is not set (!IsSet), return true (no filtering - allows seeding/admin operations)
        // When tenant is set (IsSet), filter by the current tenant ID
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            !_currentTenantService.IsSet || e.TenantId == GetCurrentTenantIdSafe());
    }

    /// <summary>
    /// Safely gets the current tenant ID, returning Guid.Empty if not set.
    /// Used by query filters to avoid exceptions.
    /// </summary>
    private Guid GetCurrentTenantIdSafe()
    {
        return _currentTenantService.IsSet ? _currentTenantService.TenantId : Guid.Empty;
    }

    /// <summary>
    /// Saves changes and automatically sets audit fields.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Set audit fields before saving
        SetAuditFields();

        // Ensure TenantId is set for new entities
        EnsureTenantIdSet();

        // Capture audit logs before saving (snapshot the change tracking state)
        var auditLogs = CaptureAuditLogs();

        // Dispatch domain events
        await DispatchDomainEventsAsync(cancellationToken);

        // Add audit logs to the same transaction before saving
        // This ensures both main changes and audit logs are committed together
        if (auditLogs.Any())
        {
            // Detach audit logs temporarily to avoid circular tracking
            foreach (var auditLog in auditLogs)
            {
                Entry(auditLog).State = EntityState.Detached;
            }

            AuditLogs.AddRange(auditLogs);
        }

        // Save all changes in a single transaction
        var result = await base.SaveChangesAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Sets audit fields (CreatedDate, ModifiedDate, CreatedBy, ModifiedBy) for tracked entities.
    /// </summary>
    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<Entity>();
        // Get user ID (nullable long) - will be null for system operations like seeding
        var currentUserId = _currentUserService.IsAuthenticated ? _currentUserService.UserId : (long?)null;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.GetType().GetProperty("CreatedDate")?.SetValue(entry.Entity, DateTime.UtcNow);
                    entry.Entity.GetType().GetProperty("CreatedBy")?.SetValue(entry.Entity, currentUserId);
                    break;

                case EntityState.Modified:
                    entry.Entity.GetType().GetProperty("ModifiedDate")?.SetValue(entry.Entity, DateTime.UtcNow);
                    entry.Entity.GetType().GetProperty("ModifiedBy")?.SetValue(entry.Entity, currentUserId);
                    break;
            }
        }
    }

    /// <summary>
    /// Ensures TenantId is set for all new entities.
    /// </summary>
    private void EnsureTenantIdSet()
    {
        if (!_currentTenantService.IsSet)
            return;

        var addedEntries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in addedEntries)
        {
            // Skip if entity is Tenant itself
            if (entry.Entity is Tenant)
                continue;

            // Set TenantId if not already set
            if (entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.GetType().GetProperty("TenantId")?
                    .SetValue(entry.Entity, _currentTenantService.TenantId);
            }
        }
    }

    /// <summary>
    /// Dispatches domain events before saving changes.
    /// </summary>
    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        domainEntities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Captures audit logs for entity changes.
    /// </summary>
    private List<AuditLog> CaptureAuditLogs()
    {
        var auditLogs = new List<AuditLog>();

        if (!_currentTenantService.IsSet)
            return auditLogs;

        var entries = ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .Where(e => !(e.Entity is AuditLog)) // Don't audit the audit logs themselves
            .ToList();

        foreach (var entry in entries)
        {
            var entityType = entry.Entity.GetType().Name;
            var tableName = entry.Metadata.GetTableName() ?? entityType;
            var entityId = GetEntityId(entry.Entity);
            var currentUserId = _currentUserService.UserId;
            var currentUsername = _currentUserService.Username ?? "System";

            AuditEventType eventType;
            string? oldValuesJson = null;
            string? newValuesJson = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    eventType = AuditEventType.Create;
                    newValuesJson = SerializeEntity(entry.CurrentValues);
                    break;

                case EntityState.Modified:
                    eventType = AuditEventType.Update;
                    oldValuesJson = SerializeEntity(entry.OriginalValues);
                    newValuesJson = SerializeEntity(entry.CurrentValues);
                    break;

                case EntityState.Deleted:
                    eventType = AuditEventType.Delete;
                    oldValuesJson = SerializeEntity(entry.OriginalValues);
                    break;

                default:
                    continue;
            }

            var auditLog = AuditLog.CreateEntityChange(
                tenantId: _currentTenantService.TenantId,
                eventType: eventType,
                entityType: entityType,
                entityId: entityId,
                userId: currentUserId,
                username: currentUsername,
                tableName: tableName,
                oldValues: oldValuesJson,
                newValues: newValuesJson
            );

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }

    /// <summary>
    /// Gets the entity ID as a long value.
    /// </summary>
    private long GetEntityId(Entity entity)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        if (idProperty == null)
            return 0;

        var idValue = idProperty.GetValue(entity);
        if (idValue is long longId)
            return longId;
        if (idValue is int intId)
            return intId;

        return 0;
    }

    /// <summary>
    /// Serializes entity property values to JSON.
    /// </summary>
    private string SerializeEntity(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues values)
    {
        var dictionary = new Dictionary<string, object?>();

        foreach (var property in values.Properties)
        {
            // Skip sensitive properties
            if (IsSensitiveProperty(property.Name))
                continue;

            var value = values[property];

            // Handle special types
            if (value is DateTime dateTime)
                dictionary[property.Name] = dateTime.ToString("O");
            else if (value is Guid guid)
                dictionary[property.Name] = guid.ToString();
            else if (value is byte[] bytes)
                dictionary[property.Name] = $"<binary data: {bytes.Length} bytes>";
            else
                dictionary[property.Name] = value;
        }

        return JsonSerializer.Serialize(dictionary, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
    }

    /// <summary>
    /// Determines if a property contains sensitive data that should not be audited.
    /// </summary>
    private bool IsSensitiveProperty(string propertyName)
    {
        var sensitiveProperties = new[]
        {
            "Password", "PasswordHash", "PasswordSalt", "Secret", "Token",
            "ApiKey", "PrivateKey", "CreditCard", "SSN", "TaxId"
        };

        return sensitiveProperties.Any(s =>
            propertyName.Contains(s, StringComparison.OrdinalIgnoreCase));
    }
}
