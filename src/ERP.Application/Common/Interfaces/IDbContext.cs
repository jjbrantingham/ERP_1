using ERP.Domain.AUDIT.Entities;
using ERP.Domain.BILL.Entities;
using ERP.Domain.Common.Entities;
using ERP.Domain.CRM.Entities;
using ERP.Domain.FIN.Entities;
using ERP.Domain.HR.Entities;
using ERP.Domain.Identity.Entities;
using ERP.Domain.PM.Entities;
using ERP.Domain.TE.Entities;
using ERP.Domain.VM.Entities;
using ERP.Domain.WF.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Database context interface for Application layer.
/// Provides access to DbSets without creating a dependency on Infrastructure.
/// Used for complex queries and operations that span multiple aggregates.
/// </summary>
public interface IDbContext
{
    // Identity & Tenancy
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }

    // Project Management
    DbSet<Project> Projects { get; }
    DbSet<WBSItem> WBSItems { get; }
    DbSet<Contract> Contracts { get; }

    // Human Resources
    DbSet<Employee> Employees { get; }
    DbSet<ResourceType> ResourceTypes { get; }
    DbSet<Rate> Rates { get; }

    // CRM
    DbSet<Client> Clients { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Note> Notes { get; }

    // Vendor Management
    DbSet<Vendor> Vendors { get; }
    DbSet<VendorContact> VendorContacts { get; }
    DbSet<VendorNote> VendorNotes { get; }

    // Time & Expense
    DbSet<Timesheet> Timesheets { get; }
    DbSet<TimesheetEntry> TimesheetEntries { get; }
    DbSet<ExpenseReport> ExpenseReports { get; }
    DbSet<ExpenseItem> ExpenseItems { get; }

    // Financial
    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryLine> JournalEntryLines { get; }

    // Billing
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLineItem> InvoiceLineItems { get; }
    DbSet<Payment> Payments { get; }

    // Workflow
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowStep> WorkflowSteps { get; }
    DbSet<WorkflowInstance> WorkflowInstances { get; }
    DbSet<WorkflowStepInstance> WorkflowStepInstances { get; }

    // Audit
    DbSet<AuditLog> AuditLogs { get; }

    /// <summary>
    /// Gets the DbSet for the specified entity type.
    /// </summary>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
