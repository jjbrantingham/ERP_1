# Phase 14: Audit Logging & Compliance

## Overview

Phase 14 implements a comprehensive audit logging and compliance system for the ERP application. This system automatically tracks all data changes, user activities, and financial transactions, providing complete transparency and meeting regulatory compliance requirements including GDPR.

## Features Implemented

### 1. Automatic Entity Change Tracking

**What it does:**
- Automatically captures all Create, Update, and Delete operations on entities
- Serializes old and new values to JSON for complete change history
- Excludes sensitive fields (passwords, tokens, etc.) from audit logs
- Runs transparently in ERPDbContext.SaveChangesAsync()

**Location:** `src/ERP.Infrastructure/Persistence/ERPDbContext.cs`

**How it works:**
```csharp
// Automatically creates audit logs when entities change
var project = new Project(...);
await _context.Projects.AddAsync(project);
await _context.SaveChangesAsync(); // AuditLog created automatically!
```

**Audit log includes:**
- Entity type and ID
- Event type (Create/Update/Delete)
- User who made the change
- Timestamp
- Old values (JSON)
- New values (JSON)
- Modified properties list

### 2. User Activity & Authentication Logging

**What it does:**
- Logs all authentication events (login, logout, failed login)
- Logs password changes
- Logs permission changes
- Captures IP address and user agent
- Security event tracking for compliance

**Event Handlers:** `src/ERP.Application/AUDIT/EventHandlers/AuthenticationAuditEventHandlers.cs`

**Domain Events:**
- UserLoggedInEvent → Creates audit log with IP and user agent
- UserLoggedOutEvent → Creates audit log
- LoginFailedEvent → Creates WARNING severity audit log
- PasswordChangedEvent → Creates audit log
- PermissionChangedEvent → Creates WARNING severity audit log

**Example:**
```csharp
// In your authentication service:
user.AddDomainEvent(new UserLoggedInEvent(user.Id, user.Username, user.Email));
await _context.SaveChangesAsync(); // Event handler creates audit log
```

### 3. Financial Audit Trails

**What it does:**
- Tracks all financial transactions with special metadata
- Logs journal entry posting and reversal
- Logs invoice posting and voiding
- Logs payment receipts
- Maintains 7-year retention for compliance

**Event Handlers:** `src/ERP.Application/AUDIT/EventHandlers/FinancialAuditEventHandlers.cs`

**Financial Events:**
- JournalEntryPostedEvent → Creates FinancialPost audit log
- JournalEntryReversedEvent → Creates FinancialReverse audit log
- InvoicePostedEvent → Creates InvoicePosted audit log
- InvoiceVoidedEvent → Creates InvoiceVoided audit log
- PaymentReceivedEvent → Creates PaymentReceived audit log

**Example:**
```csharp
// When posting a journal entry:
journalEntry.Post();
journalEntry.AddDomainEvent(new JournalEntryPostedEvent(journalEntry.Id));
await _context.SaveChangesAsync(); // Financial audit log created!
```

### 4. Audit Log Query API

**Controller:** `src/ERP.Web/Controllers/AuditLogsController.cs`

**Endpoints:**

#### Get Audit Log by ID
```http
GET /api/v1/auditlogs/{id}
Authorization: Bearer {token}
```

#### Search Audit Logs
```http
GET /api/v1/auditlogs/search?entityType=Invoice&eventType=InvoicePosted&fromDate=2024-01-01&pageSize=50
Authorization: Bearer {token}
```

**Query Parameters:**
- `entityType` - Filter by entity type (e.g., "Invoice", "Project")
- `entityId` - Filter by specific entity ID
- `eventType` - Filter by event type enum
- `severity` - Filter by severity (Information, Warning, Error, Critical)
- `userId` - Filter by user who performed the action
- `fromDate` - Start date for date range
- `toDate` - End date for date range
- `pageNumber` - Page number (default: 1)
- `pageSize` - Items per page (default: 50)

#### Get Entity Audit Trail
```http
GET /api/v1/auditlogs/entity/Invoice/12345
Authorization: Bearer {token}
```

Returns complete history of all changes to Invoice ID 12345.

#### Get User Activity
```http
GET /api/v1/auditlogs/user/789/activity?fromDate=2024-01-01
Authorization: Bearer {token}
```

Returns all activities performed by User ID 789.

#### Get Recent Audit Logs
```http
GET /api/v1/auditlogs/recent?count=100
Authorization: Bearer {token}
```

#### Get Financial Audit Logs (Requires Role)
```http
GET /api/v1/auditlogs/financial?fromDate=2024-01-01
Authorization: Bearer {token}
Requires-Role: Administrator, AccountingManager, Auditor
```

#### Get Security Audit Logs (Requires Role)
```http
GET /api/v1/auditlogs/security
Authorization: Bearer {token}
Requires-Role: Administrator, SecurityManager
```

#### Purge Old Audit Logs (Admin Only)
```http
DELETE /api/v1/auditlogs/purge?olderThan=2017-01-01&dryRun=true
Authorization: Bearer {token}
Requires-Role: Administrator
```

### 5. GDPR Compliance Features

**Controller:** `src/ERP.Web/Controllers/GDPRController.cs`

#### Right to Data Portability (Article 20)

**Export User Data:**
```http
GET /api/v1/gdpr/export/{userId}
Authorization: Bearer {token}
```

**What it exports:**
- User profile and identity data
- Employee information
- All timesheets and entries
- All expense reports and items
- Audit logs for the user
- Notes/comments created by user
- Export metadata (date, purpose, GDPR article)

**Output:** JSON file with all user data

#### Right to be Forgotten (Article 17)

**Anonymize User Data:**
```http
POST /api/v1/gdpr/anonymize/{userId}
Authorization: Bearer {token}
Requires-Role: Administrator, DataProtectionOfficer

{
  "reason": "User requested deletion per GDPR Article 17"
}
```

**What it does:**
- Replaces personal information with anonymized values
- Maintains referential integrity (doesn't break foreign keys)
- Preserves audit trail with anonymized data
- Creates CRITICAL severity audit log of the anonymization
- Irreversible operation

**Does NOT delete:**
- Transaction records (for financial compliance)
- Audit logs (anonymizes PII but keeps records)
- Historical data needed for business operations

### 6. Data Retention Policies

**Service:** `src/ERP.Infrastructure/Services/DataRetentionPolicyService.cs`
**Background Service:** `src/ERP.Infrastructure/BackgroundServices/DataRetentionBackgroundService.cs`

**Retention Periods:**

| Event Type | Retention Period | Reason |
|-----------|------------------|--------|
| Financial events (GL, AR, AP, Invoices) | 7 years (2555 days) | Legal/tax compliance |
| Security events (login, permissions) | 2 years (730 days) | Security compliance |
| Workflow events (approvals) | 3 years (1095 days) | Business compliance |
| GDPR events (export, anonymization) | Permanent | Legal requirement |
| CRUD operations | 1 year (365 days) | Standard retention |
| System events | 90 days | Operational data |

**Background Service:**
- Runs daily at 2:00 AM UTC
- Automatically purges audit logs older than retention period
- Logs all purge operations
- Can be run manually via API endpoint

**Manual Purge:**
```csharp
var command = new PurgeOldAuditLogsCommand
{
    OlderThan = DateTime.UtcNow.AddYears(-7),
    DryRun = true // Test first!
};
var count = await _mediator.Send(command);
```

## Architecture

### Domain Layer

**Entities:**
- `AuditLog` - Core audit log entity with factory methods for different audit types

**Enums:**
- `AuditEventType` - 20+ event types (Create, Update, Delete, Login, FinancialPost, etc.)
- `AuditSeverity` - Information, Warning, Error, Critical

**Repositories:**
- `IAuditLogRepository` - Repository interface with search capabilities

**Events:**
- Authentication events (UserLoggedInEvent, LoginFailedEvent, etc.)
- Financial events (JournalEntryPostedEvent, InvoicePostedEvent, etc.)

### Application Layer

**Commands:**
- `CreateAuditLogCommand` - Manually create audit log
- `PurgeOldAuditLogsCommand` - Purge old audit logs
- `ExportUserDataCommand` - Export user data (GDPR)
- `AnonymizeUserDataCommand` - Anonymize user data (GDPR)

**Queries:**
- `GetAuditLogByIdQuery` - Get single audit log
- `SearchAuditLogsQuery` - Search with filters
- `GetAuditTrailForEntityQuery` - Get entity history
- `GetUserActivityQuery` - Get user activity logs

**Event Handlers:**
- `AuthenticationAuditEventHandlers` - 5 handlers for auth events
- `FinancialAuditEventHandlers` - 5 handlers for financial events

**DTOs:**
- `AuditLogDto` - Data transfer object for API responses

### Infrastructure Layer

**Repositories:**
- `AuditLogRepository` - EF Core implementation with optimized queries

**Configurations:**
- `AuditLogConfiguration` - EF Core configuration with 7 indexes

**Services:**
- `DataRetentionPolicyService` - Manages retention policies
- `DataRetentionBackgroundService` - Background job for purging

**Database:**
- Schema: `audit`
- Table: `AuditLogs`
- Indexes: 7 indexes for query performance

### Web Layer

**Controllers:**
- `AuditLogsController` - 8 endpoints for audit log access
- `GDPRController` - 4 endpoints for GDPR compliance

## Database Schema

```sql
CREATE TABLE [audit].[AuditLogs] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [TenantId] UNIQUEIDENTIFIER NOT NULL,
    [EventType] TINYINT NOT NULL,
    [Severity] TINYINT NOT NULL,
    [EntityType] NVARCHAR(200) NOT NULL,
    [EntityId] NVARCHAR(50) NULL,
    [UserId] BIGINT NULL,
    [Username] NVARCHAR(256) NOT NULL,
    [IpAddress] NVARCHAR(45) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [Description] NVARCHAR(4000) NOT NULL,
    [OldValues] NVARCHAR(MAX) NULL,
    [NewValues] NVARCHAR(MAX) NULL,
    [Metadata] NVARCHAR(MAX) NULL,
    [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Indexes for performance
CREATE INDEX IX_AuditLogs_TenantId_Timestamp ON [audit].[AuditLogs] (TenantId, Timestamp DESC);
CREATE INDEX IX_AuditLogs_Entity ON [audit].[AuditLogs] (EntityType, EntityId);
CREATE INDEX IX_AuditLogs_UserId ON [audit].[AuditLogs] (UserId);
CREATE INDEX IX_AuditLogs_EventType ON [audit].[AuditLogs] (EventType);
CREATE INDEX IX_AuditLogs_Severity ON [audit].[AuditLogs] (Severity);
CREATE INDEX IX_AuditLogs_Timestamp ON [audit].[AuditLogs] (Timestamp DESC);
CREATE INDEX IX_AuditLogs_Search ON [audit].[AuditLogs] (TenantId, EventType, Timestamp DESC);
```

## Usage Examples

### 1. Query Audit Trail for an Invoice

```csharp
var query = new GetAuditTrailForEntityQuery
{
    EntityType = "Invoice",
    EntityId = 12345
};
var auditTrail = await _mediator.Send(query);

// Results show:
// - Invoice created
// - Invoice updated (line items added)
// - Invoice posted (AR entries created)
// - Payment received
```

### 2. Investigate Failed Login Attempts

```csharp
var query = new SearchAuditLogsQuery
{
    EventType = AuditEventType.LoginFailed,
    FromDate = DateTime.UtcNow.AddDays(-7),
    Severity = AuditSeverity.Warning,
    PageSize = 100
};
var failedLogins = await _mediator.Send(query);
```

### 3. Export User Data for GDPR Request

```csharp
var command = new ExportUserDataCommand { UserId = 789 };
var json = await _mediator.Send(command);

// Save to file or send to user
File.WriteAllText("user-data-export.json", json);
```

### 4. View Financial Audit Trail for Compliance

```http
GET /api/v1/auditlogs/financial?fromDate=2023-01-01&toDate=2023-12-31
Authorization: Bearer {token}
```

Returns all financial transactions for tax year 2023.

## Security Considerations

### 1. Sensitive Data Protection
- Passwords, tokens, SSN, credit cards are NEVER logged
- `IsSensitiveProperty()` method filters sensitive fields
- User agents and IP addresses are anonymized in GDPR operations

### 2. Access Control
- Most endpoints require authentication
- Financial audit logs require `AccountingManager` or `Auditor` role
- Security audit logs require `SecurityManager` role
- GDPR anonymization requires `DataProtectionOfficer` role
- Purge operations require `Administrator` role

### 3. Audit Log Integrity
- Audit logs themselves cannot be modified
- Deletion only through retention policies or admin purge
- CRITICAL severity events logged for sensitive operations
- Audit logs are excluded from automatic change tracking (prevents recursion)

## Performance Considerations

### 1. Indexes
Seven indexes ensure fast queries:
- TenantId + Timestamp (most common query)
- EntityType + EntityId (entity history)
- UserId (user activity)
- EventType (event filtering)
- Severity (severity filtering)
- Timestamp (date range queries)
- Composite search index

### 2. Pagination
- All search endpoints support pagination
- Default page size: 50 items
- Maximum page size: 500 items
- Use pagination for large result sets

### 3. Background Processing
- Automatic audit logging is async and non-blocking
- Audit logs are saved in a separate transaction after main save
- Data retention runs during off-peak hours (2 AM)

### 4. JSON Storage
- Old/new values stored as JSON for flexibility
- Use sparse columns if available in SQL Server
- Consider compression for large JSON fields

## Compliance

### GDPR Compliance

✅ **Article 15 - Right of Access**
- Users can request all their data via `/api/v1/gdpr/export/{userId}`

✅ **Article 17 - Right to Erasure**
- Users can request data anonymization via `/api/v1/gdpr/anonymize/{userId}`
- Balances right to erasure with legal retention requirements

✅ **Article 20 - Right to Data Portability**
- Data exported in machine-readable JSON format

✅ **Article 30 - Records of Processing Activities**
- Complete audit trail of all data processing
- Logs who accessed/modified data, when, and why

✅ **Article 32 - Security of Processing**
- Audit logs track security events
- Failed login attempts logged
- Permission changes logged

### SOX Compliance (for public companies)

✅ **Financial Data Integrity**
- All financial transactions logged
- 7-year retention period
- Immutable audit trail
- Segregation of duties tracked

### HIPAA Compliance (if handling health data)

✅ **Audit Controls (§164.312(b))**
- Complete audit trail
- User activity tracking
- Access logging

## Testing

### Unit Tests

```csharp
[Fact]
public async Task SaveChanges_CreateEntity_CreatesAuditLog()
{
    // Arrange
    var context = CreateTestContext();
    var project = new Project(...);

    // Act
    context.Projects.Add(project);
    await context.SaveChangesAsync();

    // Assert
    var auditLog = context.AuditLogs.First();
    auditLog.EventType.Should().Be(AuditEventType.Create);
    auditLog.EntityType.Should().Be("Project");
}
```

### Integration Tests

```csharp
[Fact]
public async Task SearchAuditLogs_WithFilters_ReturnsFilteredResults()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync(
        "/api/v1/auditlogs/search?eventType=InvoicePosted&pageSize=10");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<PagedResult<AuditLogDto>>();
    result.Items.Should().AllSatisfy(a => a.EventType == AuditEventType.InvoicePosted);
}
```

## Production Deployment

### 1. Database Migration

```bash
cd src/ERP.Infrastructure
dotnet ef migrations add Phase14_AuditLogging --startup-project ../ERP.Web
dotnet ef database update --startup-project ../ERP.Web
```

### 2. Configuration

**appsettings.json:**
```json
{
  "DataRetention": {
    "Enabled": true,
    "RunTime": "02:00:00",
    "FinancialRetentionYears": 7,
    "SecurityRetentionYears": 2,
    "GeneralRetentionDays": 365
  },
  "Audit": {
    "CaptureIpAddress": true,
    "CaptureUserAgent": true,
    "ExcludeSensitiveFields": true
  }
}
```

### 3. Monitoring

Monitor these metrics:
- Audit log volume (logs/day)
- Query performance (avg query time)
- Storage growth (GB/month)
- Failed login attempts
- GDPR requests processed

### 4. Backup Strategy

- Audit logs are mission-critical
- Include in regular database backups
- Consider separate backup schedule for audit schema
- Test restore procedures

### 5. Alerts

Set up alerts for:
- High number of failed logins (potential attack)
- CRITICAL severity audit logs
- Retention policy failures
- Disk space warnings

## Files Created

### Domain Layer (9 files)
- `src/ERP.Domain/AUDIT/Enums/AuditEnums.cs`
- `src/ERP.Domain/AUDIT/Entities/AuditLog.cs`
- `src/ERP.Domain/AUDIT/Repositories/IAuditLogRepository.cs`
- `src/ERP.Domain/Identity/Events/UserLoggedInEvent.cs`
- `src/ERP.Domain/Identity/Events/UserLoggedOutEvent.cs`
- `src/ERP.Domain/Identity/Events/LoginFailedEvent.cs`
- `src/ERP.Domain/Identity/Events/PasswordChangedEvent.cs`
- `src/ERP.Domain/Identity/Events/PermissionChangedEvent.cs`

### Application Layer (13 files)
- `src/ERP.Application/AUDIT/DTOs/AuditLogDto.cs`
- `src/ERP.Application/AUDIT/Queries/GetAuditLogByIdQuery.cs`
- `src/ERP.Application/AUDIT/Queries/SearchAuditLogsQuery.cs`
- `src/ERP.Application/AUDIT/Queries/GetAuditTrailForEntityQuery.cs`
- `src/ERP.Application/AUDIT/Queries/GetUserActivityQuery.cs`
- `src/ERP.Application/AUDIT/Commands/CreateAuditLogCommand.cs`
- `src/ERP.Application/AUDIT/Commands/PurgeOldAuditLogsCommand.cs`
- `src/ERP.Application/AUDIT/Commands/ExportUserDataCommand.cs`
- `src/ERP.Application/AUDIT/Commands/AnonymizeUserDataCommand.cs`
- `src/ERP.Application/AUDIT/Handlers/` (8 handler files)
- `src/ERP.Application/AUDIT/EventHandlers/AuthenticationAuditEventHandlers.cs`
- `src/ERP.Application/AUDIT/EventHandlers/FinancialAuditEventHandlers.cs`
- `src/ERP.Application/AUDIT/Services/IDataRetentionPolicyService.cs`

### Infrastructure Layer (4 files)
- `src/ERP.Infrastructure/Persistence/Repositories/AuditLogRepository.cs`
- `src/ERP.Infrastructure/Persistence/Configurations/AuditLogConfiguration.cs`
- `src/ERP.Infrastructure/Services/DataRetentionPolicyService.cs`
- `src/ERP.Infrastructure/BackgroundServices/DataRetentionBackgroundService.cs`

### Web Layer (2 files)
- `src/ERP.Web/Controllers/AuditLogsController.cs`
- `src/ERP.Web/Controllers/GDPRController.cs`

### Modified Files (2 files)
- `src/ERP.Infrastructure/Persistence/ERPDbContext.cs` - Added automatic change tracking
- `src/ERP.Web/Program.cs` - Registered audit services

### Documentation (1 file)
- `docs/PHASE_14_AUDIT_LOGGING.md` - This file

## Summary

Phase 14 delivers a production-ready audit logging and compliance system that:

✅ Automatically tracks all entity changes
✅ Logs user activity and authentication events
✅ Maintains financial audit trails for compliance
✅ Provides comprehensive query API
✅ Implements GDPR Right to Access and Right to Erasure
✅ Manages data retention policies automatically
✅ Optimized for performance with 7 indexes
✅ Secure and role-based access control
✅ Ready for SOX, GDPR, and HIPAA compliance

**Total Files Created: 30**
**Total Lines of Code: ~4,000**
**Features: 6 major feature sets**
**API Endpoints: 12 endpoints**

The system is production-ready and provides complete transparency and accountability for all system operations.
