# Add Database Migration

Create and review EF Core migrations following best practices.

## What This Skill Does

Helps create, review, and apply Entity Framework Core migrations:

1. **Generate migration** from model changes
2. **Review migration code** for correctness
3. **Check for data loss risks**
4. **Validate indexes and constraints**
5. **Apply migration** to database
6. **Create rollback plan**

## Migration Workflow

### 1. Before Creating Migration

✅ **Verify model changes are complete**:
- All entity configurations updated
- Value objects properly mapped
- Indexes defined
- Foreign keys configured
- Multi-tenancy support added

✅ **Update DbContext**:
```csharp
// Add DbSet for new entities
public DbSet<Invoice> Invoices { get; set; }
public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }

// Apply entity configurations in OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Apply all configurations from assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ERPDbContext).Assembly);
}
```

### 2. Create Migration

```bash
# From Infrastructure project directory
cd src/ERP.Infrastructure

# Create migration with descriptive name
dotnet ef migrations add AddBillingModule --startup-project ../ERP.Web

# Common migration naming patterns:
# - AddXxxModule (for new modules)
# - AddXxxEntity (for new entity)
# - UpdateXxxSchema (for schema changes)
# - AddXxxIndexes (for performance)
# - FixXxxConstraint (for bug fixes)
```

### 3. Review Migration

Migration files are created in `src/ERP.Infrastructure/Persistence/Migrations/`:
- `{timestamp}_MigrationName.cs` - Main migration class
- `{timestamp}_MigrationName.Designer.cs` - Snapshot (don't edit)
- `ERPDbContextModelSnapshot.cs` - Current model state

✅ **Review Up() method**:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ✅ GOOD: Schema specified
    migrationBuilder.EnsureSchema(name: "bill");

    // ✅ GOOD: Complete table definition
    migrationBuilder.CreateTable(
        name: "Invoices",
        schema: "bill",
        columns: table => new
        {
            InvoiceId = table.Column<long>(nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            TenantId = table.Column<Guid>(nullable: false),  // ✅ Required for multi-tenancy
            InvoiceNumber = table.Column<string>(maxLength: 50, nullable: false),
            InvoiceDate = table.Column<DateTime>(nullable: false),
            TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),  // ✅ Precision specified
            Status = table.Column<byte>(nullable: false),
            CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
            ModifiedDate = table.Column<DateTime>(nullable: true),
            RowVersion = table.Column<byte[]>(rowVersion: true, nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
            table.ForeignKey(
                name: "FK_Invoices_Clients",
                column: x => x.ClientId,
                principalSchema: "crm",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Restrict);  // ✅ Explicit delete behavior
        });

    // ✅ GOOD: Indexes for performance
    migrationBuilder.CreateIndex(
        name: "IX_Invoices_TenantId",
        schema: "bill",
        table: "Invoices",
        column: "TenantId");

    migrationBuilder.CreateIndex(
        name: "IX_Invoices_TenantId_InvoiceNumber",
        schema: "bill",
        table: "Invoices",
        columns: new[] { "TenantId", "InvoiceNumber" },
        unique: true);  // ✅ Tenant-scoped uniqueness

    migrationBuilder.CreateIndex(
        name: "IX_Invoices_ClientId",
        schema: "bill",
        table: "Invoices",
        column: "ClientId");

    migrationBuilder.CreateIndex(
        name: "IX_Invoices_Status",
        schema: "bill",
        table: "Invoices",
        column: "Status");
}
```

### 4. Migration Review Checklist

#### Schema and Tables

- [ ] Schema name is correct (matches bounded context)
- [ ] Table names are plural and clear
- [ ] Primary keys defined
- [ ] All columns have appropriate types
- [ ] Nullable/Not Nullable correct
- [ ] Default values specified where needed

#### Multi-Tenancy

- [ ] `TenantId` column exists and is NOT NULL
- [ ] `TenantId` indexed
- [ ] Unique constraints include `TenantId` (e.g., TenantId + InvoiceNumber)

#### Data Types

- [ ] Money fields use `decimal(18,2)`
- [ ] Strings have appropriate `MaxLength`
- [ ] Dates use `DateTime` or `DateTimeOffset`
- [ ] Enums use `byte` or `int`
- [ ] RowVersion for optimistic concurrency

#### Foreign Keys

- [ ] Foreign keys defined
- [ ] Referential actions appropriate (Cascade, Restrict, SetNull)
- [ ] Schema specified for cross-schema references

#### Indexes

- [ ] Primary keys indexed (automatic)
- [ ] Foreign keys indexed
- [ ] `TenantId` indexed
- [ ] Frequently queried columns indexed
- [ ] Unique constraints for business keys (with TenantId)
- [ ] Composite indexes for common queries

#### Audit Fields

- [ ] `CreatedDate` with default value `GETUTCDATE()`
- [ ] `ModifiedDate` nullable
- [ ] `RowVersion` for concurrency
- [ ] Consider `CreatedBy` and `ModifiedBy` if needed

#### Data Loss Risk

- [ ] No `DropColumn` without data migration
- [ ] No `DropTable` for production data
- [ ] Type changes compatible (e.g., varchar(50) → varchar(100) OK, reverse is NOT)
- [ ] Unique constraint additions won't fail on existing data

### 5. Custom Migration Code

Sometimes you need to add custom SQL:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Generated code...

    // Custom SQL for data migration
    migrationBuilder.Sql(@"
        -- Migrate old data to new structure
        UPDATE bill.Invoices
        SET Status = 1  -- Draft
        WHERE Status IS NULL
    ");

    // Or use Sql method for complex operations
    migrationBuilder.Sql(@"
        CREATE NONCLUSTERED INDEX IX_Invoices_DueDate_Status
        ON bill.Invoices (DueDate, Status)
        INCLUDE (TotalAmount, AmountPaid)
        WHERE Status IN (1, 2)  -- Only Active and Overdue
    ");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    // Reverse the custom changes
    migrationBuilder.Sql(@"
        DROP INDEX IX_Invoices_DueDate_Status ON bill.Invoices
    ");
}
```

### 6. Apply Migration

```bash
# Review migration first!

# Apply to database
dotnet ef database update --startup-project ../ERP.Web

# Apply specific migration
dotnet ef database update MigrationName --startup-project ../ERP.Web

# Rollback to previous migration
dotnet ef database update PreviousMigrationName --startup-project ../ERP.Web

# Generate SQL script without applying (for review/production)
dotnet ef migrations script --startup-project ../ERP.Web --output migration.sql

# Generate script from specific migration to latest
dotnet ef migrations script PreviousMigration --startup-project ../ERP.Web
```

### 7. Production Deployment

For production, NEVER apply migrations directly. Instead:

```bash
# 1. Generate SQL script
dotnet ef migrations script LastProductionMigration --startup-project ../ERP.Web --output migration.sql

# 2. Review the SQL script
# 3. Test in staging environment
# 4. Create rollback script (Down migration)
dotnet ef migrations script CurrentMigration LastProductionMigration --startup-project ../ERP.Web --output rollback.sql

# 5. Apply in production during maintenance window
# 6. Keep rollback script ready
```

## Common Migration Patterns

### Adding New Module

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Ensure schema exists
    migrationBuilder.EnsureSchema(name: "bill");

    // 2. Create main table
    migrationBuilder.CreateTable(
        name: "Invoices",
        schema: "bill",
        ...
    );

    // 3. Create related tables
    migrationBuilder.CreateTable(
        name: "InvoiceLineItems",
        schema: "bill",
        ...
    );

    // 4. Create indexes
    migrationBuilder.CreateIndex(...);
}
```

### Adding Column to Existing Table

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "Notes",
        schema: "bill",
        table: "Invoices",
        type: "nvarchar(4000)",
        maxLength: 4000,
        nullable: true);

    // Add index if needed
    migrationBuilder.CreateIndex(
        name: "IX_Invoices_Notes",
        schema: "bill",
        table: "Invoices",
        column: "Notes");
}
```

### Renaming Column (with data preservation)

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.RenameColumn(
        name: "Description",
        schema: "bill",
        table: "Invoices",
        newName: "Notes");

    // Rename index if needed
    migrationBuilder.RenameIndex(
        name: "IX_Invoices_Description",
        schema: "bill",
        table: "Invoices",
        newName: "IX_Invoices_Notes");
}
```

### Adding Foreign Key

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<long>(
        name: "ProjectId",
        schema: "bill",
        table: "Invoices",
        nullable: true);

    migrationBuilder.CreateIndex(
        name: "IX_Invoices_ProjectId",
        schema: "bill",
        table: "Invoices",
        column: "ProjectId");

    migrationBuilder.AddForeignKey(
        name: "FK_Invoices_Projects",
        schema: "bill",
        table: "Invoices",
        column: "ProjectId",
        principalSchema: "pm",
        principalTable: "Projects",
        principalColumn: "ProjectId",
        onDelete: ReferentialAction.Restrict);
}
```

### Data Migration

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Add new column
    migrationBuilder.AddColumn<string>(
        name: "InvoiceNumber",
        schema: "bill",
        table: "Invoices",
        maxLength: 50,
        nullable: true);  // Temporarily nullable

    // Migrate data
    migrationBuilder.Sql(@"
        UPDATE bill.Invoices
        SET InvoiceNumber = 'INV-' + CONVERT(VARCHAR, InvoiceId)
        WHERE InvoiceNumber IS NULL
    ");

    // Make column required
    migrationBuilder.AlterColumn<string>(
        name: "InvoiceNumber",
        schema: "bill",
        table: "Invoices",
        maxLength: 50,
        nullable: false);

    // Add unique constraint
    migrationBuilder.CreateIndex(
        name: "IX_Invoices_TenantId_InvoiceNumber",
        schema: "bill",
        table: "Invoices",
        columns: new[] { "TenantId", "InvoiceNumber" },
        unique: true);
}
```

## Troubleshooting

### Migration fails with foreign key constraint

```bash
# Check existing data violates new constraint
SELECT * FROM bill.Invoices WHERE ClientId NOT IN (SELECT ClientId FROM crm.Clients)

# Option 1: Fix data first
UPDATE bill.Invoices SET ClientId = NULL WHERE ClientId NOT IN (SELECT ClientId FROM crm.Clients)

# Option 2: Remove the migration and fix model
dotnet ef migrations remove --startup-project ../ERP.Web
```

### Remove last migration

```bash
# Only if not applied to database!
dotnet ef migrations remove --startup-project ../ERP.Web

# If already applied, create a new migration to reverse it
dotnet ef database update PreviousMigration --startup-project ../ERP.Web
dotnet ef migrations remove --startup-project ../ERP.Web
```

### Reset database (DEV ONLY!)

```bash
# WARNING: Deletes all data!
dotnet ef database drop --startup-project ../ERP.Web
dotnet ef database update --startup-project ../ERP.Web
```

## Best Practices

- ✅ **ALWAYS** review migration code before applying
- ✅ **ALWAYS** backup production database before migrations
- ✅ **ALWAYS** test migrations in staging first
- ✅ **ALWAYS** have rollback plan ready
- ✅ **ALWAYS** use schemas for bounded contexts
- ✅ **ALWAYS** include TenantId in unique constraints
- ✅ **ALWAYS** index foreign keys
- ✅ **ALWAYS** specify decimal precision for money
- ✅ **NEVER** apply migrations directly in production
- ✅ **NEVER** delete columns with data without backup
- ✅ **NEVER** make breaking changes without data migration
- ✅ Use descriptive migration names
- ✅ Keep migrations small and focused
- ✅ Document complex migrations with comments
- ✅ Test rollback (Down) migrations
