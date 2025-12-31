# Database Migrations

This directory contains Entity Framework Core migrations for the ERP application.

## Prerequisites

1. .NET 9 SDK installed
2. SQL Server (LocalDB, Express, or full version)
3. EF Core tools installed globally:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

## Connection String

Update the connection string in `src/ERP.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ERPDatabase;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

For Azure SQL:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Database=ERPDatabase;User ID=your-username;Password=your-password;Encrypt=True;TrustServerCertificate=False;"
  }
}
```

## Applying Migrations

### Option 1: Automatic Migration (Recommended for Development)

Set the `AUTO_MIGRATE` environment variable or add to `appsettings.Development.json`:

```json
{
  "AutoMigrate": true
}
```

Then run the application:
```bash
cd src/ERP.Web
dotnet run
```

The database will be created and seeded automatically on startup.

### Option 2: Manual Migration using EF Core Tools

From the `ERP.Infrastructure` directory:

```bash
cd src/ERP.Infrastructure

# Apply migrations
dotnet ef database update --startup-project ../ERP.Web

# Or specify a specific migration
dotnet ef database update InitialCreate --startup-project ../ERP.Web
```

### Option 3: Generate SQL Script for Manual Execution

Generate a SQL script to review before applying:

```bash
cd src/ERP.Infrastructure

# Generate script for all migrations
dotnet ef migrations script --startup-project ../ERP.Web --output migrations.sql

# Generate script for specific migration
dotnet ef migrations script 0 InitialCreate --startup-project ../ERP.Web --output initial.sql
```

Then execute the script in SQL Server Management Studio or Azure Data Studio.

## Creating New Migrations

When you add new entities or modify existing ones:

```bash
cd src/ERP.Infrastructure

# Create a new migration
dotnet ef migrations add MigrationName --startup-project ../ERP.Web

# Apply the migration
dotnet ef database update --startup-project ../ERP.Web
```

## Removing the Last Migration

If you need to remove the last migration (before applying it):

```bash
cd src/ERP.Infrastructure
dotnet ef migrations remove --startup-project ../ERP.Web
```

## Database Schema

The database is organized into schemas:

- **common**: Shared entities (Tenants)
- **identity**: Authentication and authorization (Users, Roles, UserRoles, RolePermissions)
- **pm**: Project Management (Projects, WBS, Contracts) - To be added
- **hr**: Human Resources (Employees, Resource Types, Rates) - To be added
- **crm**: Customer Relationship Management (Clients, Contacts) - To be added
- **vm**: Vendor Management (Vendors, Contractors) - To be added
- **te**: Time & Expense (Timesheets, Expense Reports) - To be added
- **fin**: Financial Management (Accounts, Journal Entries, AR/AP) - To be added
- **bill**: Billing (Invoices, Payments) - To be added
- **rpt**: Reporting (Reports, Dashboards) - To be added

## Initial Seed Data

The `InitialCreate` migration seeds the following data:

### Default Tenant
- Company Name: "Default Company"
- Subdomain: "default"
- Max Users: 100
- Subscription Plan: "Enterprise"

### Roles
1. **System Administrator** - Full system access (all permissions)
2. **Tenant Administrator** - Full tenant access
3. **Project Manager** - Project, resource, and schedule management
4. **Finance Manager** - Financial operations and billing
5. **HR Manager** - Employee and HR management
6. **Accountant** - Accounting and financial reports
7. **Employee** - Time and expense entry
8. **Client** - Limited external access
9. **Viewer** - Read-only access

### Default System Administrator
- **Username**: `admin`
- **Email**: `admin@erp.local`
- **Password**: `Admin@123` (**CHANGE THIS IMMEDIATELY!**)
- **Role**: System Administrator

**⚠️ SECURITY WARNING**: Change the default admin password immediately after first login!

## Troubleshooting

### Migration already applied error
```bash
# Reset to a specific migration
dotnet ef database update PreviousMigrationName --startup-project ../ERP.Web

# Or drop the database and start fresh (DESTROYS ALL DATA!)
dotnet ef database drop --startup-project ../ERP.Web
dotnet ef database update --startup-project ../ERP.Web
```

### Connection errors
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure firewall allows SQL Server connections
- For LocalDB: `sqllocaldb start mssqllocaldb`

### Permission errors
- Ensure the database user has CREATE DATABASE permission
- For Windows Authentication, run as administrator if needed
- For Azure SQL, ensure firewall rules allow your IP

## Production Deployment

For production deployments:

1. **Never use AutoMigrate in production**
2. Generate SQL scripts and review them:
   ```bash
   dotnet ef migrations script --startup-project ../ERP.Web --output prod-migration.sql
   ```
3. Have a DBA review the script
4. Execute during a maintenance window
5. Test in a staging environment first
6. Always backup the database before applying migrations

## Multi-Tenancy Notes

- All entities (except Tenant) are tenant-isolated via `TenantId`
- Global query filters automatically filter by current tenant
- Unique constraints include `TenantId` (e.g., username is unique per tenant)
- Seeded data includes a default tenant; production systems should create tenants via API

## Next Migrations

Future migrations will add tables for:
- Project Management (Projects, WBS, Contracts, Resource Allocations)
- Human Resources (Employees, Resource Types, Rates)
- CRM (Clients, Contacts, Notes)
- Vendor Management (Vendors, Contractors)
- Time & Expense (Timesheets, Expense Reports)
- Financial Management (Chart of Accounts, Journal Entries, AR/AP)
- Billing (Invoices, Payments)
- Reporting (Reports, Dashboards)
