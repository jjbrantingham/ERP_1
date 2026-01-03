# Database Schema Review Skill

## Purpose
Review database schema changes for correctness, performance, and compliance with ERP requirements.

## Review Areas

### 1. Multi-Tenancy Compliance
- [ ] Every table has `TenantId` column (UNIQUEIDENTIFIER NOT NULL)
- [ ] Composite unique indexes include `TenantId`
- [ ] Global query filters configured for tenant isolation
- [ ] Foreign keys respect tenant boundaries
- [ ] No cross-tenant data leakage possible

### 2. Primary Keys & Identity
- [ ] Primary keys use BIGINT IDENTITY (not GUID for performance)
- [ ] Column named `[Table]Id` (e.g., `ProjectId`, `InvoiceId`)
- [ ] Clustered index on primary key
- [ ] No natural keys used as primary keys

### 3. Foreign Keys
- [ ] All relationships have foreign key constraints
- [ ] Named consistently: `FK_[ChildTable]_[ParentTable]_[Column]`
- [ ] Proper ON DELETE behavior:
  - CASCADE for owned entities (1:many composition)
  - RESTRICT for references (many:1 association)
  - SET NULL only when business logic allows
- [ ] Indexes on foreign key columns

### 4. Indexes
- [ ] Indexes on foreign keys
- [ ] Indexes on frequently queried columns
- [ ] Composite indexes for common query patterns
- [ ] Include `TenantId` in indexes for multi-tenant queries
- [ ] No duplicate or redundant indexes
- [ ] Consider covering indexes for frequent queries

**Common Index Patterns:**
```sql
-- Multi-tenant lookup
CREATE INDEX IX_[Table]_TenantId_[Column]
ON [schema].[Table](TenantId, [Column]);

-- Status queries
CREATE INDEX IX_[Table]_Status
ON [schema].[Table](Status) WHERE Status <> [ClosedStatus];

-- Date range queries
CREATE INDEX IX_[Table]_Date
ON [schema].[Table](DateColumn DESC);

-- Composite for common filters
CREATE INDEX IX_[Table]_TenantId_Status_Date
ON [schema].[Table](TenantId, Status, DateColumn)
INCLUDE (CommonlySelectedColumn);
```

### 5. Data Types
- [ ] Money: `DECIMAL(18,2)` or `DECIMAL(19,4)` for precision
- [ ] Dates: `DATETIME2` (not DATETIME)
- [ ] Text: Appropriate VARCHAR/NVARCHAR lengths
- [ ] Booleans: `BIT NOT NULL` with default
- [ ] Enums: `TINYINT` or `SMALLINT` with check constraint
- [ ] IDs: `BIGINT` for primary keys, foreign keys
- [ ] Tenant: `UNIQUEIDENTIFIER NOT NULL`

### 6. Audit Columns
Every table should have:
```sql
CreatedBy NVARCHAR(256) NOT NULL,
CreatedDate DATETIME2 NOT NULL DEFAULT (GETUTCDATE()),
LastModifiedBy NVARCHAR(256) NULL,
LastModifiedDate DATETIME2 NULL,
RowVersion ROWVERSION
```

### 7. Constraints
- [ ] NOT NULL where appropriate (avoid nullable key fields)
- [ ] Check constraints for enums or value ranges
- [ ] Default values for status, dates, booleans
- [ ] Unique constraints for business keys (include TenantId)

**Example Constraints:**
```sql
-- Enum check constraint
CONSTRAINT CK_[Table]_Status
CHECK (Status IN (0, 1, 2, 3, 4))

-- Business rule constraint
CONSTRAINT CK_Invoice_DueDate
CHECK (DueDate >= InvoiceDate)

-- Unique business key
CONSTRAINT UQ_[Table]_TenantId_BusinessKey
UNIQUE (TenantId, [BusinessKey])
```

### 8. Schema Organization
- [ ] Tables in appropriate schema:
  - `pm` - Project Management
  - `hr` - Human Resources
  - `crm` - Customer Relationship Management
  - `vm` - Vendor Management
  - `te` - Time & Expense
  - `fin` - Financial Management
  - `bill` - Billing
  - `rpt` - Reporting
  - `audit` - Audit logs
- [ ] No tables in `dbo` schema

### 9. Performance Considerations
- [ ] Large tables partitioned if needed (> 10M rows)
- [ ] Archive strategy for historical data
- [ ] No NVARCHAR(MAX) unless truly needed
- [ ] Appropriate column order (fixed-length before variable-length)
- [ ] Statistics auto-update enabled

### 10. Data Integrity
- [ ] Referential integrity enforced
- [ ] No orphaned records possible
- [ ] Cascade deletes configured correctly
- [ ] Soft delete pattern where needed (IsDeleted bit)

## Financial Table Requirements

For tables with financial data:

### Double-Entry Bookkeeping Tables
```sql
-- Journal Entries must balance
CONSTRAINT CK_JournalEntry_Balanced
CHECK (TotalDebits = TotalCredits)

-- Money columns
DebitAmount DECIMAL(19,4) NOT NULL DEFAULT 0,
CreditAmount DECIMAL(19,4) NOT NULL DEFAULT 0,

-- Account validation
CONSTRAINT FK_JournalLine_Account
FOREIGN KEY (AccountId) REFERENCES fin.Accounts(AccountId)
```

### Audit Requirements
- [ ] No DELETE on financial transactions (soft delete only)
- [ ] Full audit trail (who, when, what changed)
- [ ] Immutable once posted (Status check constraint)

## Migration Review Checklist

### Before Applying Migration

1. **Data Loss Check**
   - [ ] No columns dropped without backup
   - [ ] No data truncation (reducing column size)
   - [ ] Nullable to NOT NULL has default or data fill

2. **Backward Compatibility**
   - [ ] Can application run with old schema during deployment?
   - [ ] Are column additions nullable initially?
   - [ ] Is there a rollback script?

3. **Performance Impact**
   - [ ] Adding indexes online (ONLINE = ON)
   - [ ] Large table alterations during maintenance window
   - [ ] No table locks during business hours

4. **Testing**
   - [ ] Migration tested on copy of production data
   - [ ] Performance tested
   - [ ] Rollback tested

## Common Issues & Fixes

### Issue: Missing TenantId
```sql
-- Add TenantId
ALTER TABLE [schema].[Table]
ADD TenantId UNIQUEIDENTIFIER NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

-- Add to indexes
CREATE INDEX IX_[Table]_TenantId_[Column]
ON [schema].[Table](TenantId, [Column]);
```

### Issue: Missing Foreign Key Index
```sql
CREATE INDEX IX_[Table]_[ParentTable]Id
ON [schema].[Table]([ParentTable]Id);
```

### Issue: Wrong Data Type for Money
```sql
-- Change to proper decimal
ALTER TABLE [schema].[Table]
ALTER COLUMN Amount DECIMAL(18,2) NOT NULL;
```

## Review Output Format

Provide structured feedback:

### 1. Summary
- Overall assessment
- Critical issues count
- Warnings count

### 2. Critical Issues
Issues that MUST be fixed before deployment:
- Missing TenantId
- Missing foreign keys
- Data loss risk
- Performance killers

### 3. Warnings
Issues that SHOULD be addressed:
- Missing indexes
- Suboptimal data types
- Missing constraints

### 4. Recommendations
Best practice improvements:
- Additional indexes
- Better naming
- Performance optimizations

### 5. Migration Script Review
- SQL review
- Rollback script
- Deployment notes

## Example Usage

```
User: /database-schema-review "Adding new tables for recurring billing"

Claude: I'll review the database schema for recurring billing...

CRITICAL ISSUES:
1. Table `RecurringBilling` missing TenantId column
2. No foreign key index on `SubscriptionId`

WARNINGS:
1. Column `Amount` using FLOAT instead of DECIMAL
2. Missing index on `NextBillingDate`

[Full review with fixes]
```

## Related Skills
- architecture-review
- add-migration
- migration-review
- optimize-queries
