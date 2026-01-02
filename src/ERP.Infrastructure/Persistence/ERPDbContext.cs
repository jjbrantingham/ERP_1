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
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence;

/// <summary>
/// Main database context for the ERP application.
/// Implements multi-tenancy with global query filters.
/// </summary>
public class ERPDbContext : DbContext
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
    /// </summary>
    private void SetGlobalQueryFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : Entity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            e.TenantId == _currentTenantService.TenantId);
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

        // Dispatch domain events
        await DispatchDomainEventsAsync(cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Sets audit fields (CreatedDate, ModifiedDate, CreatedBy, ModifiedBy) for tracked entities.
    /// </summary>
    private void SetAuditFields()
    {
        var entries = ChangeTracker.Entries<Entity>();
        var currentUsername = _currentUserService.Username ?? "System";

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.GetType().GetProperty("CreatedDate")?.SetValue(entry.Entity, DateTime.UtcNow);
                    entry.Entity.GetType().GetProperty("CreatedBy")?.SetValue(entry.Entity, currentUsername);
                    break;

                case EntityState.Modified:
                    entry.Entity.GetType().GetProperty("ModifiedDate")?.SetValue(entry.Entity, DateTime.UtcNow);
                    entry.Entity.GetType().GetProperty("ModifiedBy")?.SetValue(entry.Entity, currentUsername);
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
}
