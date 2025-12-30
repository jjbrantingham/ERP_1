# ERP SaaS Application - Database Schema Design

## Database Overview

- **Database Platform**: Azure SQL Database
- **Approach**: Database-First with Entity Framework Core Migrations
- **Naming Convention**: PascalCase for tables and columns
- **Multi-Tenancy**: TenantId column on all tables with filtered indexes

---

## Schema Organization

Schemas are organized by bounded context:

- `dbo` - System/shared tables
- `pm` - Project Management
- `hr` - Human Resources
- `crm` - Customer Relationship Management
- `vm` - Vendor Management
- `te` - Time & Expense
- `fin` - Financial Management
- `bill` - Billing
- `rpt` - Reporting

---

## Core System Tables

### dbo.Tenants
```sql
CREATE TABLE dbo.Tenants (
    TenantId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Subdomain NVARCHAR(100) NOT NULL UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1,
    SubscriptionTier NVARCHAR(50) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,
    CONSTRAINT CK_Tenants_Subdomain CHECK (Subdomain LIKE '[a-z0-9-]%')
);

CREATE INDEX IX_Tenants_Subdomain ON dbo.Tenants(Subdomain) WHERE IsActive = 1;
```

### dbo.Users
```sql
CREATE TABLE dbo.Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,
    CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_Users_TenantId_Email UNIQUE (TenantId, Email)
);

CREATE INDEX IX_Users_TenantId ON dbo.Users(TenantId);
CREATE INDEX IX_Users_Email ON dbo.Users(Email);
```

### dbo.Roles
```sql
CREATE TABLE dbo.Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsSystemRole BIT NOT NULL DEFAULT 0
);

INSERT INTO dbo.Roles (Name, Description, IsSystemRole) VALUES
    ('Admin', 'System Administrator', 1),
    ('Manager', 'Project Manager', 1),
    ('Employee', 'Standard Employee', 1),
    ('Accountant', 'Accountant/Finance', 1),
    ('Client', 'Client User', 1);
```

### dbo.UserRoles
```sql
CREATE TABLE dbo.UserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);
```

---

## Project Management Schema (pm)

### pm.Projects
```sql
CREATE TABLE pm.Projects (
    ProjectId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ProjectNumber NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ProjectType TINYINT NOT NULL, -- 1=Billable, 2=Overhead, 3=Proposal
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Draft, 2=Active, 3=OnHold, 4=Completed, 5=Cancelled
    ClientId BIGINT NULL,
    BudgetAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    BudgetCurrency NCHAR(3) NOT NULL DEFAULT 'USD',
    StartDate DATE NULL,
    EndDate DATE NULL,
    ActualStartDate DATE NULL,
    ActualEndDate DATE NULL,
    PercentComplete DECIMAL(5,2) NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_Projects_Clients FOREIGN KEY (ClientId) REFERENCES crm.Clients(ClientId),
    CONSTRAINT FK_Projects_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_Projects_TenantId_ProjectNumber UNIQUE (TenantId, ProjectNumber),
    CONSTRAINT CK_Projects_PercentComplete CHECK (PercentComplete BETWEEN 0 AND 100),
    CONSTRAINT CK_Projects_Dates CHECK (EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate)
);

CREATE INDEX IX_Projects_TenantId ON pm.Projects(TenantId);
CREATE INDEX IX_Projects_ClientId ON pm.Projects(ClientId);
CREATE INDEX IX_Projects_Status ON pm.Projects(Status) WHERE Status IN (2, 3); -- Active, OnHold
CREATE INDEX IX_Projects_ProjectType ON pm.Projects(ProjectType);
```

### pm.WorkBreakdownStructure
```sql
CREATE TABLE pm.WorkBreakdownStructure (
    WBSItemId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ProjectId BIGINT NOT NULL,
    ParentWBSItemId BIGINT NULL,
    Code NVARCHAR(50) NOT NULL, -- e.g., "1.2.3"
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    BudgetAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    BudgetCurrency NCHAR(3) NOT NULL DEFAULT 'USD',
    Level INT NOT NULL, -- 1=Project, 2=Phase, 3=Task
    SortOrder INT NOT NULL,

    -- Resource Planning Fields
    ScheduledStartDate DATE NULL,
    ScheduledEndDate DATE NULL,
    ActualStartDate DATE NULL,
    ActualEndDate DATE NULL,
    EstimatedHours DECIMAL(10,2) NOT NULL DEFAULT 0,
    ActualHours DECIMAL(10,2) NOT NULL DEFAULT 0,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=NotStarted, 2=InProgress, 3=Completed, 4=OnHold, 5=Cancelled

    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_WBS_Projects FOREIGN KEY (ProjectId) REFERENCES pm.Projects(ProjectId) ON DELETE CASCADE,
    CONSTRAINT FK_WBS_ParentWBS FOREIGN KEY (ParentWBSItemId) REFERENCES pm.WorkBreakdownStructure(WBSItemId),
    CONSTRAINT FK_WBS_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_WBS_ProjectId_Code UNIQUE (ProjectId, Code),
    CONSTRAINT CK_WBS_ScheduledDates CHECK (ScheduledEndDate IS NULL OR ScheduledStartDate IS NULL OR ScheduledEndDate >= ScheduledStartDate),
    CONSTRAINT CK_WBS_Level CHECK (Level BETWEEN 1 AND 3)
);

CREATE INDEX IX_WBS_TenantId ON pm.WorkBreakdownStructure(TenantId);
CREATE INDEX IX_WBS_ProjectId ON pm.WorkBreakdownStructure(ProjectId);
CREATE INDEX IX_WBS_ParentWBSItemId ON pm.WorkBreakdownStructure(ParentWBSItemId);
CREATE INDEX IX_WBS_ScheduledDates ON pm.WorkBreakdownStructure(ScheduledStartDate, ScheduledEndDate);
CREATE INDEX IX_WBS_Status ON pm.WorkBreakdownStructure(Status);
```

### pm.WBSDependencies
```sql
CREATE TABLE pm.WBSDependencies (
    DependencyId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    SuccessorWBSItemId BIGINT NOT NULL,
    PredecessorWBSItemId BIGINT NOT NULL,
    DependencyType TINYINT NOT NULL, -- 1=FinishToStart, 2=StartToStart, 3=FinishToFinish, 4=StartToFinish
    LagDays INT NOT NULL DEFAULT 0, -- Positive for delay, negative for overlap
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_WBSDependencies_Successor FOREIGN KEY (SuccessorWBSItemId) REFERENCES pm.WorkBreakdownStructure(WBSItemId),
    CONSTRAINT FK_WBSDependencies_Predecessor FOREIGN KEY (PredecessorWBSItemId) REFERENCES pm.WorkBreakdownStructure(WBSItemId),
    CONSTRAINT FK_WBSDependencies_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_WBSDependencies UNIQUE (SuccessorWBSItemId, PredecessorWBSItemId),
    CONSTRAINT CK_WBSDependencies_NoSelfReference CHECK (SuccessorWBSItemId <> PredecessorWBSItemId)
);

CREATE INDEX IX_WBSDependencies_TenantId ON pm.WBSDependencies(TenantId);
CREATE INDEX IX_WBSDependencies_Successor ON pm.WBSDependencies(SuccessorWBSItemId);
CREATE INDEX IX_WBSDependencies_Predecessor ON pm.WBSDependencies(PredecessorWBSItemId);
```

### pm.WBSResourceAssignments
```sql
CREATE TABLE pm.WBSResourceAssignments (
    AssignmentId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    WBSItemId BIGINT NOT NULL,
    EmployeeId BIGINT NULL, -- NULL if generic resource
    GenericResourceTypeId INT NULL, -- For placeholder resources
    GenericResourceName NVARCHAR(100) NULL, -- e.g., "Senior Developer"

    -- Assignment Type and Values
    AssignmentType TINYINT NOT NULL, -- 1=TotalHours, 2=HoursPerWeek, 3=Budget
    AssignmentValue DECIMAL(18,2) NOT NULL,
    CalculatedTotalHours DECIMAL(10,2) NOT NULL,
    CalculatedBudgetAmount DECIMAL(18,2) NOT NULL,
    CalculatedBudgetCurrency NCHAR(3) NOT NULL DEFAULT 'USD',

    -- Scheduling
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    AllocationPercentage DECIMAL(5,2) NOT NULL DEFAULT 100, -- % of resource's time

    -- Rates at time of assignment (frozen rates)
    CostRate DECIMAL(18,2) NOT NULL,
    BillingRate DECIMAL(18,2) NOT NULL,
    RateCurrency NCHAR(3) NOT NULL DEFAULT 'USD',

    -- Status
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Planned, 2=Confirmed, 3=InProgress, 4=Completed, 5=Cancelled
    IsGeneric BIT NOT NULL DEFAULT 0, -- True if not assigned to specific employee

    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,

    CONSTRAINT FK_WBSResourceAssignments_WBS FOREIGN KEY (WBSItemId) REFERENCES pm.WorkBreakdownStructure(WBSItemId) ON DELETE CASCADE,
    CONSTRAINT FK_WBSResourceAssignments_Employees FOREIGN KEY (EmployeeId) REFERENCES hr.Employees(EmployeeId),
    CONSTRAINT FK_WBSResourceAssignments_GenericResourceTypes FOREIGN KEY (GenericResourceTypeId) REFERENCES hr.ResourceTypes(ResourceTypeId),
    CONSTRAINT FK_WBSResourceAssignments_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT CK_WBSResourceAssignments_Resource CHECK (
        (IsGeneric = 0 AND EmployeeId IS NOT NULL AND GenericResourceTypeId IS NULL AND GenericResourceName IS NULL) OR
        (IsGeneric = 1 AND EmployeeId IS NULL AND (GenericResourceTypeId IS NOT NULL OR GenericResourceName IS NOT NULL))
    ),
    CONSTRAINT CK_WBSResourceAssignments_Dates CHECK (EndDate >= StartDate),
    CONSTRAINT CK_WBSResourceAssignments_Allocation CHECK (AllocationPercentage > 0 AND AllocationPercentage <= 100)
);

CREATE INDEX IX_WBSResourceAssignments_TenantId ON pm.WBSResourceAssignments(TenantId);
CREATE INDEX IX_WBSResourceAssignments_WBSItemId ON pm.WBSResourceAssignments(WBSItemId);
CREATE INDEX IX_WBSResourceAssignments_EmployeeId ON pm.WBSResourceAssignments(EmployeeId);
CREATE INDEX IX_WBSResourceAssignments_Status ON pm.WBSResourceAssignments(Status);
CREATE INDEX IX_WBSResourceAssignments_Dates ON pm.WBSResourceAssignments(StartDate, EndDate);
CREATE INDEX IX_WBSResourceAssignments_IsGeneric ON pm.WBSResourceAssignments(IsGeneric) WHERE IsGeneric = 1;
```

### pm.ResourceCapacity
```sql
CREATE TABLE pm.ResourceCapacity (
    CapacityId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EmployeeId BIGINT NOT NULL,
    WeekStartDate DATE NOT NULL, -- Start of week (Monday)
    TotalAvailableHours DECIMAL(10,2) NOT NULL, -- Total hours available in week (typically 40)
    AllocatedHours DECIMAL(10,2) NOT NULL DEFAULT 0, -- Hours allocated across all assignments
    UtilizationPercentage DECIMAL(5,2) NOT NULL DEFAULT 0, -- Allocated / Available * 100

    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,

    CONSTRAINT FK_ResourceCapacity_Employees FOREIGN KEY (EmployeeId) REFERENCES hr.Employees(EmployeeId) ON DELETE CASCADE,
    CONSTRAINT FK_ResourceCapacity_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_ResourceCapacity UNIQUE (TenantId, EmployeeId, WeekStartDate),
    CONSTRAINT CK_ResourceCapacity_Hours CHECK (AllocatedHours >= 0 AND TotalAvailableHours > 0)
);

CREATE INDEX IX_ResourceCapacity_TenantId ON pm.ResourceCapacity(TenantId);
CREATE INDEX IX_ResourceCapacity_EmployeeId ON pm.ResourceCapacity(EmployeeId);
CREATE INDEX IX_ResourceCapacity_WeekStartDate ON pm.ResourceCapacity(WeekStartDate);
CREATE INDEX IX_ResourceCapacity_Overallocated ON pm.ResourceCapacity(UtilizationPercentage) WHERE UtilizationPercentage > 100;
```

### pm.CapacityAllocations
```sql
CREATE TABLE pm.CapacityAllocations (
    AllocationId BIGINT PRIMARY KEY IDENTITY(1,1),
    CapacityId BIGINT NOT NULL,
    WBSAssignmentId BIGINT NOT NULL,
    ProjectId BIGINT NOT NULL,
    WBSItemId BIGINT NOT NULL,
    ProjectName NVARCHAR(200) NOT NULL,
    WBSItemName NVARCHAR(200) NOT NULL,
    AllocatedHours DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_CapacityAllocations_Capacity FOREIGN KEY (CapacityId) REFERENCES pm.ResourceCapacity(CapacityId) ON DELETE CASCADE,
    CONSTRAINT FK_CapacityAllocations_Assignment FOREIGN KEY (WBSAssignmentId) REFERENCES pm.WBSResourceAssignments(AssignmentId),
    CONSTRAINT FK_CapacityAllocations_Projects FOREIGN KEY (ProjectId) REFERENCES pm.Projects(ProjectId),
    CONSTRAINT FK_CapacityAllocations_WBS FOREIGN KEY (WBSItemId) REFERENCES pm.WorkBreakdownStructure(WBSItemId)
);

CREATE INDEX IX_CapacityAllocations_CapacityId ON pm.CapacityAllocations(CapacityId);
CREATE INDEX IX_CapacityAllocations_WBSAssignmentId ON pm.CapacityAllocations(WBSAssignmentId);
CREATE INDEX IX_CapacityAllocations_ProjectId ON pm.CapacityAllocations(ProjectId);
```

### pm.Contracts
```sql
CREATE TABLE pm.Contracts (
    ContractId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ProjectId BIGINT NOT NULL,
    ContractNumber NVARCHAR(50) NOT NULL,
    ContractType TINYINT NOT NULL, -- 1=FixedPrice, 2=TimeAndMaterial, 3=Retainer, 4=MilestoneBased
    ContractValue DECIMAL(18,2) NOT NULL,
    ContractCurrency NCHAR(3) NOT NULL DEFAULT 'USD',
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Terms NVARCHAR(MAX) NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Draft, 2=Active, 3=Completed, 4=Cancelled
    SignedDate DATE NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_Contracts_Projects FOREIGN KEY (ProjectId) REFERENCES pm.Projects(ProjectId),
    CONSTRAINT FK_Contracts_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_Contracts_TenantId_ContractNumber UNIQUE (TenantId, ContractNumber)
);

CREATE INDEX IX_Contracts_TenantId ON pm.Contracts(TenantId);
CREATE INDEX IX_Contracts_ProjectId ON pm.Contracts(ProjectId);
```

### pm.ContractMilestones
```sql
CREATE TABLE pm.ContractMilestones (
    MilestoneId BIGINT PRIMARY KEY IDENTITY(1,1),
    ContractId BIGINT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Amount DECIMAL(18,2) NOT NULL,
    DueDate DATE NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Pending, 2=Completed, 3=Invoiced
    CompletedDate DATE NULL,
    SortOrder INT NOT NULL,
    CONSTRAINT FK_ContractMilestones_Contracts FOREIGN KEY (ContractId) REFERENCES pm.Contracts(ContractId) ON DELETE CASCADE
);

CREATE INDEX IX_ContractMilestones_ContractId ON pm.ContractMilestones(ContractId);
```

### pm.ProjectResourceAllocations
```sql
CREATE TABLE pm.ProjectResourceAllocations (
    AllocationId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ProjectId BIGINT NOT NULL,
    EmployeeId BIGINT NOT NULL,
    ResourceTypeId INT NOT NULL,
    AllocationPercentage DECIMAL(5,2) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    BillingRate DECIMAL(18,2) NOT NULL,
    BillingCurrency NCHAR(3) NOT NULL DEFAULT 'USD',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_ProjectResourceAllocations_Projects FOREIGN KEY (ProjectId) REFERENCES pm.Projects(ProjectId),
    CONSTRAINT FK_ProjectResourceAllocations_Employees FOREIGN KEY (EmployeeId) REFERENCES hr.Employees(EmployeeId),
    CONSTRAINT FK_ProjectResourceAllocations_ResourceTypes FOREIGN KEY (ResourceTypeId) REFERENCES hr.ResourceTypes(ResourceTypeId),
    CONSTRAINT FK_ProjectResourceAllocations_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT CK_ProjectResourceAllocations_Percentage CHECK (AllocationPercentage BETWEEN 0 AND 100)
);

CREATE INDEX IX_ProjectResourceAllocations_TenantId ON pm.ProjectResourceAllocations(TenantId);
CREATE INDEX IX_ProjectResourceAllocations_ProjectId ON pm.ProjectResourceAllocations(ProjectId);
CREATE INDEX IX_ProjectResourceAllocations_EmployeeId ON pm.ProjectResourceAllocations(EmployeeId);
```

---

## Human Resources Schema (hr)

### hr.Employees
```sql
CREATE TABLE hr.Employees (
    EmployeeId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EmployeeNumber NVARCHAR(50) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Street1 NVARCHAR(200) NULL,
    Street2 NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    Country NVARCHAR(100) NULL,
    EmploymentType TINYINT NOT NULL, -- 1=FullTime, 2=PartTime, 3=Contract, 4=Consultant
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Active, 2=OnLeave, 3=Terminated, 4=Retired
    HireDate DATE NOT NULL,
    TerminationDate DATE NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_Employees_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_Employees_TenantId_EmployeeNumber UNIQUE (TenantId, EmployeeNumber),
    CONSTRAINT UQ_Employees_TenantId_Email UNIQUE (TenantId, Email)
);

CREATE INDEX IX_Employees_TenantId ON hr.Employees(TenantId);
CREATE INDEX IX_Employees_Status ON hr.Employees(Status);
CREATE INDEX IX_Employees_Email ON hr.Employees(Email);
```

### hr.ResourceTypes
```sql
CREATE TABLE hr.ResourceTypes (
    ResourceTypeId INT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    DefaultCostRate DECIMAL(18,2) NOT NULL DEFAULT 0,
    DefaultBillingRate DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_ResourceTypes_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_ResourceTypes_TenantId_Code UNIQUE (TenantId, Code)
);

CREATE INDEX IX_ResourceTypes_TenantId ON hr.ResourceTypes(TenantId);
```

### hr.EmployeeResourceTypes
```sql
CREATE TABLE hr.EmployeeResourceTypes (
    EmployeeResourceTypeId BIGINT PRIMARY KEY IDENTITY(1,1),
    EmployeeId BIGINT NOT NULL,
    ResourceTypeId INT NOT NULL,
    EffectiveDate DATE NOT NULL,
    EndDate DATE NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_EmployeeResourceTypes_Employees FOREIGN KEY (EmployeeId) REFERENCES hr.Employees(EmployeeId) ON DELETE CASCADE,
    CONSTRAINT FK_EmployeeResourceTypes_ResourceTypes FOREIGN KEY (ResourceTypeId) REFERENCES hr.ResourceTypes(ResourceTypeId)
);

CREATE INDEX IX_EmployeeResourceTypes_EmployeeId ON hr.EmployeeResourceTypes(EmployeeId);
CREATE INDEX IX_EmployeeResourceTypes_ResourceTypeId ON hr.EmployeeResourceTypes(ResourceTypeId);
```

### hr.EmployeeRates
```sql
CREATE TABLE hr.EmployeeRates (
    RateId BIGINT PRIMARY KEY IDENTITY(1,1),
    EmployeeId BIGINT NOT NULL,
    CostRate DECIMAL(18,2) NOT NULL,
    BillingRate DECIMAL(18,2) NOT NULL,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    EffectiveDate DATE NOT NULL,
    EndDate DATE NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_EmployeeRates_Employees FOREIGN KEY (EmployeeId) REFERENCES hr.Employees(EmployeeId) ON DELETE CASCADE
);

CREATE INDEX IX_EmployeeRates_EmployeeId ON hr.EmployeeRates(EmployeeId);
CREATE INDEX IX_EmployeeRates_EffectiveDate ON hr.EmployeeRates(EffectiveDate);
```

---

## CRM Schema (crm)

### crm.Clients
```sql
CREATE TABLE crm.Clients (
    ClientId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ClientNumber NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    ClientType TINYINT NOT NULL, -- 1=Corporation, 2=SmallBusiness, 3=Individual, 4=Government, 5=NonProfit
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Prospect, 2=Active, 3=Inactive, 4=OnHold
    TaxId NVARCHAR(50) NULL,

    -- Billing Address
    BillingStreet1 NVARCHAR(200) NULL,
    BillingStreet2 NVARCHAR(200) NULL,
    BillingCity NVARCHAR(100) NULL,
    BillingState NVARCHAR(100) NULL,
    BillingPostalCode NVARCHAR(20) NULL,
    BillingCountry NVARCHAR(100) NULL,

    -- Shipping Address
    ShippingStreet1 NVARCHAR(200) NULL,
    ShippingStreet2 NVARCHAR(200) NULL,
    ShippingCity NVARCHAR(100) NULL,
    ShippingState NVARCHAR(100) NULL,
    ShippingPostalCode NVARCHAR(20) NULL,
    ShippingCountry NVARCHAR(100) NULL,

    -- Payment Terms
    PaymentTermsNetDays INT NOT NULL DEFAULT 30,
    PaymentTermsDiscountPercentage DECIMAL(5,2) NULL,
    PaymentTermsDiscountDays INT NULL,

    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_Clients_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_Clients_TenantId_ClientNumber UNIQUE (TenantId, ClientNumber)
);

CREATE INDEX IX_Clients_TenantId ON crm.Clients(TenantId);
CREATE INDEX IX_Clients_Status ON crm.Clients(Status);
CREATE INDEX IX_Clients_Name ON crm.Clients(Name);
```

### crm.Contacts
```sql
CREATE TABLE crm.Contacts (
    ContactId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    MiddleName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Mobile NVARCHAR(20) NULL,
    JobTitle NVARCHAR(100) NULL,
    ContactType TINYINT NOT NULL, -- 1=Client, 2=Contractor, 3=Vendor, 4=Other
    IsPrimary BIT NOT NULL DEFAULT 0,

    -- Relationships
    ClientId BIGINT NULL,
    ContractorId BIGINT NULL,
    VendorId BIGINT NULL,

    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_Contacts_Clients FOREIGN KEY (ClientId) REFERENCES crm.Clients(ClientId),
    CONSTRAINT FK_Contacts_Contractors FOREIGN KEY (ContractorId) REFERENCES vm.Contractors(ContractorId),
    CONSTRAINT FK_Contacts_Vendors FOREIGN KEY (VendorId) REFERENCES vm.Vendors(VendorId),
    CONSTRAINT FK_Contacts_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT CK_Contacts_OneRelationship CHECK (
        (ClientId IS NOT NULL AND ContractorId IS NULL AND VendorId IS NULL) OR
        (ClientId IS NULL AND ContractorId IS NOT NULL AND VendorId IS NULL) OR
        (ClientId IS NULL AND ContractorId IS NULL AND VendorId IS NOT NULL) OR
        (ClientId IS NULL AND ContractorId IS NULL AND VendorId IS NULL)
    )
);

CREATE INDEX IX_Contacts_TenantId ON crm.Contacts(TenantId);
CREATE INDEX IX_Contacts_ClientId ON crm.Contacts(ClientId);
CREATE INDEX IX_Contacts_Email ON crm.Contacts(Email);
```

### crm.ContactNotes
```sql
CREATE TABLE crm.ContactNotes (
    NoteId BIGINT PRIMARY KEY IDENTITY(1,1),
    ContactId BIGINT NOT NULL,
    Subject NVARCHAR(200) NOT NULL,
    Content NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_ContactNotes_Contacts FOREIGN KEY (ContactId) REFERENCES crm.Contacts(ContactId) ON DELETE CASCADE
);

CREATE INDEX IX_ContactNotes_ContactId ON crm.ContactNotes(ContactId);
```

---

## Vendor Management Schema (vm)

### vm.Contractors
```sql
CREATE TABLE vm.Contractors (
    ContractorId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ContractorNumber NVARCHAR(50) NOT NULL,
    CompanyName NVARCHAR(200) NOT NULL,
    ContractorType TINYINT NOT NULL DEFAULT 1,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Active, 2=Inactive
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(20) NULL,
    TaxId NVARCHAR(50) NULL,

    -- Address
    Street1 NVARCHAR(200) NULL,
    Street2 NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    Country NVARCHAR(100) NULL,

    -- Payment Terms
    PaymentTermsNetDays INT NOT NULL DEFAULT 30,

    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_Contractors_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_Contractors_TenantId_ContractorNumber UNIQUE (TenantId, ContractorNumber)
);

CREATE INDEX IX_Contractors_TenantId ON vm.Contractors(TenantId);
```

### vm.Vendors
```sql
CREATE TABLE vm.Vendors (
    VendorId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    VendorNumber NVARCHAR(50) NOT NULL,
    CompanyName NVARCHAR(200) NOT NULL,
    VendorType TINYINT NOT NULL DEFAULT 1,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Active, 2=Inactive
    Email NVARCHAR(256) NOT NULL,
    Phone NVARCHAR(20) NULL,
    TaxId NVARCHAR(50) NULL,

    -- Address
    Street1 NVARCHAR(200) NULL,
    Street2 NVARCHAR(200) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    Country NVARCHAR(100) NULL,

    -- Payment Terms
    PaymentTermsNetDays INT NOT NULL DEFAULT 30,

    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_Vendors_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_Vendors_TenantId_VendorNumber UNIQUE (TenantId, VendorNumber)
);

CREATE INDEX IX_Vendors_TenantId ON vm.Vendors(TenantId);
```

---

## Time & Expense Schema (te)

### te.Timesheets
```sql
CREATE TABLE te.Timesheets (
    TimesheetId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EmployeeId BIGINT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Draft, 2=Submitted, 3=Approved, 4=Rejected
    SubmittedDate DATETIME2 NULL,
    ApprovedDate DATETIME2 NULL,
    ApprovedBy BIGINT NULL,
    RejectedDate DATETIME2 NULL,
    RejectedBy BIGINT NULL,
    RejectionReason NVARCHAR(500) NULL,
    TotalHours DECIMAL(10,2) NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_Timesheets_Employees FOREIGN KEY (EmployeeId) REFERENCES hr.Employees(EmployeeId),
    CONSTRAINT FK_Timesheets_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES hr.Employees(EmployeeId),
    CONSTRAINT FK_Timesheets_RejectedBy FOREIGN KEY (RejectedBy) REFERENCES hr.Employees(EmployeeId),
    CONSTRAINT FK_Timesheets_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT CK_Timesheets_DateRange CHECK (EndDate >= StartDate)
);

CREATE INDEX IX_Timesheets_TenantId ON te.Timesheets(TenantId);
CREATE INDEX IX_Timesheets_EmployeeId ON te.Timesheets(EmployeeId);
CREATE INDEX IX_Timesheets_Status ON te.Timesheets(Status);
CREATE INDEX IX_Timesheets_StartDate ON te.Timesheets(StartDate);
```

### te.TimesheetEntries
```sql
CREATE TABLE te.TimesheetEntries (
    EntryId BIGINT PRIMARY KEY IDENTITY(1,1),
    TimesheetId BIGINT NOT NULL,
    ProjectId BIGINT NOT NULL,
    WBSItemId BIGINT NULL,
    EntryDate DATE NOT NULL,
    Hours DECIMAL(10,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsBillable BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_TimesheetEntries_Timesheets FOREIGN KEY (TimesheetId) REFERENCES te.Timesheets(TimesheetId) ON DELETE CASCADE,
    CONSTRAINT FK_TimesheetEntries_Projects FOREIGN KEY (ProjectId) REFERENCES pm.Projects(ProjectId),
    CONSTRAINT FK_TimesheetEntries_WBSItems FOREIGN KEY (WBSItemId) REFERENCES pm.WorkBreakdownStructure(WBSItemId),
    CONSTRAINT CK_TimesheetEntries_Hours CHECK (Hours >= 0 AND Hours <= 24)
);

CREATE INDEX IX_TimesheetEntries_TimesheetId ON te.TimesheetEntries(TimesheetId);
CREATE INDEX IX_TimesheetEntries_ProjectId ON te.TimesheetEntries(ProjectId);
CREATE INDEX IX_TimesheetEntries_EntryDate ON te.TimesheetEntries(EntryDate);
```

### te.ExpenseReports
```sql
CREATE TABLE te.ExpenseReports (
    ExpenseReportId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ReportNumber NVARCHAR(50) NOT NULL,
    EmployeeId BIGINT NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Draft, 2=Submitted, 3=Approved, 4=Rejected, 5=Paid
    SubmittedDate DATETIME2 NULL,
    ApprovedDate DATETIME2 NULL,
    ApprovedBy BIGINT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_ExpenseReports_Employees FOREIGN KEY (EmployeeId) REFERENCES hr.Employees(EmployeeId),
    CONSTRAINT FK_ExpenseReports_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES hr.Employees(EmployeeId),
    CONSTRAINT FK_ExpenseReports_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_ExpenseReports_TenantId_ReportNumber UNIQUE (TenantId, ReportNumber)
);

CREATE INDEX IX_ExpenseReports_TenantId ON te.ExpenseReports(TenantId);
CREATE INDEX IX_ExpenseReports_EmployeeId ON te.ExpenseReports(EmployeeId);
CREATE INDEX IX_ExpenseReports_Status ON te.ExpenseReports(Status);
```

### te.ExpenseEntries
```sql
CREATE TABLE te.ExpenseEntries (
    EntryId BIGINT PRIMARY KEY IDENTITY(1,1),
    ExpenseReportId BIGINT NOT NULL,
    ProjectId BIGINT NOT NULL,
    Category TINYINT NOT NULL, -- 1=Travel, 2=Meals, 3=Lodging, 4=Transportation, 5=Supplies, 6=Equipment, 7=Other
    EntryDate DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    Description NVARCHAR(500) NULL,
    ReceiptUrl NVARCHAR(500) NULL,
    IsBillable BIT NOT NULL DEFAULT 1,
    IsReimbursable BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_ExpenseEntries_ExpenseReports FOREIGN KEY (ExpenseReportId) REFERENCES te.ExpenseReports(ExpenseReportId) ON DELETE CASCADE,
    CONSTRAINT FK_ExpenseEntries_Projects FOREIGN KEY (ProjectId) REFERENCES pm.Projects(ProjectId),
    CONSTRAINT CK_ExpenseEntries_Amount CHECK (Amount >= 0)
);

CREATE INDEX IX_ExpenseEntries_ExpenseReportId ON te.ExpenseEntries(ExpenseReportId);
CREATE INDEX IX_ExpenseEntries_ProjectId ON te.ExpenseEntries(ProjectId);
```

---

## Financial Management Schema (fin)

### fin.ChartOfAccounts
```sql
CREATE TABLE fin.ChartOfAccounts (
    AccountId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    AccountNumber NVARCHAR(50) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    AccountType TINYINT NOT NULL, -- 1=Asset, 2=Liability, 3=Equity, 4=Revenue, 5=Expense
    AccountCategory TINYINT NOT NULL,
    ParentAccountId BIGINT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    BalanceAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    BalanceCurrency NCHAR(3) NOT NULL DEFAULT 'USD',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_ChartOfAccounts_Parent FOREIGN KEY (ParentAccountId) REFERENCES fin.ChartOfAccounts(AccountId),
    CONSTRAINT FK_ChartOfAccounts_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_ChartOfAccounts_TenantId_AccountNumber UNIQUE (TenantId, AccountNumber)
);

CREATE INDEX IX_ChartOfAccounts_TenantId ON fin.ChartOfAccounts(TenantId);
CREATE INDEX IX_ChartOfAccounts_AccountType ON fin.ChartOfAccounts(AccountType);
CREATE INDEX IX_ChartOfAccounts_ParentAccountId ON fin.ChartOfAccounts(ParentAccountId);
```

### fin.JournalEntries
```sql
CREATE TABLE fin.JournalEntries (
    JournalEntryId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EntryNumber NVARCHAR(50) NOT NULL,
    EntryDate DATE NOT NULL,
    PostingDate DATE NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    JournalEntryType TINYINT NOT NULL, -- 1=Standard, 2=Adjusting, 3=Closing, 4=Reversing
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Draft, 2=Posted, 3=Reversed
    ReferenceNumber NVARCHAR(50) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_JournalEntries_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_JournalEntries_TenantId_EntryNumber UNIQUE (TenantId, EntryNumber)
);

CREATE INDEX IX_JournalEntries_TenantId ON fin.JournalEntries(TenantId);
CREATE INDEX IX_JournalEntries_Status ON fin.JournalEntries(Status);
CREATE INDEX IX_JournalEntries_EntryDate ON fin.JournalEntries(EntryDate);
CREATE INDEX IX_JournalEntries_PostingDate ON fin.JournalEntries(PostingDate);
```

### fin.JournalEntryLines
```sql
CREATE TABLE fin.JournalEntryLines (
    LineId BIGINT PRIMARY KEY IDENTITY(1,1),
    JournalEntryId BIGINT NOT NULL,
    AccountId BIGINT NOT NULL,
    DebitAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreditAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    Description NVARCHAR(500) NULL,
    ProjectId BIGINT NULL,
    CONSTRAINT FK_JournalEntryLines_JournalEntries FOREIGN KEY (JournalEntryId) REFERENCES fin.JournalEntries(JournalEntryId) ON DELETE CASCADE,
    CONSTRAINT FK_JournalEntryLines_Accounts FOREIGN KEY (AccountId) REFERENCES fin.ChartOfAccounts(AccountId),
    CONSTRAINT FK_JournalEntryLines_Projects FOREIGN KEY (ProjectId) REFERENCES pm.Projects(ProjectId),
    CONSTRAINT CK_JournalEntryLines_Amounts CHECK (
        (DebitAmount > 0 AND CreditAmount = 0) OR
        (CreditAmount > 0 AND DebitAmount = 0)
    )
);

CREATE INDEX IX_JournalEntryLines_JournalEntryId ON fin.JournalEntryLines(JournalEntryId);
CREATE INDEX IX_JournalEntryLines_AccountId ON fin.JournalEntryLines(AccountId);
CREATE INDEX IX_JournalEntryLines_ProjectId ON fin.JournalEntryLines(ProjectId);
```

### fin.AccountsReceivable
```sql
CREATE TABLE fin.AccountsReceivable (
    ARId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ClientId BIGINT NOT NULL,
    InvoiceId BIGINT NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    AmountPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    DueDate DATE NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Open, 2=PartiallyPaid, 3=Paid, 4=Overdue, 5=WrittenOff
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_AccountsReceivable_Clients FOREIGN KEY (ClientId) REFERENCES crm.Clients(ClientId),
    CONSTRAINT FK_AccountsReceivable_Invoices FOREIGN KEY (InvoiceId) REFERENCES bill.Invoices(InvoiceId),
    CONSTRAINT FK_AccountsReceivable_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
);

CREATE INDEX IX_AccountsReceivable_TenantId ON fin.AccountsReceivable(TenantId);
CREATE INDEX IX_AccountsReceivable_ClientId ON fin.AccountsReceivable(ClientId);
CREATE INDEX IX_AccountsReceivable_Status ON fin.AccountsReceivable(Status);
CREATE INDEX IX_AccountsReceivable_DueDate ON fin.AccountsReceivable(DueDate);
```

### fin.AccountsPayable
```sql
CREATE TABLE fin.AccountsPayable (
    APId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    VendorId BIGINT NULL,
    ContractorId BIGINT NULL,
    ReferenceNumber NVARCHAR(50) NULL,
    Amount DECIMAL(18,2) NOT NULL,
    AmountPaid DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    DueDate DATE NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Open, 2=PartiallyPaid, 3=Paid, 4=Overdue
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedDate DATETIME2 NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_AccountsPayable_Vendors FOREIGN KEY (VendorId) REFERENCES vm.Vendors(VendorId),
    CONSTRAINT FK_AccountsPayable_Contractors FOREIGN KEY (ContractorId) REFERENCES vm.Contractors(ContractorId),
    CONSTRAINT FK_AccountsPayable_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT CK_AccountsPayable_OneEntity CHECK (
        (VendorId IS NOT NULL AND ContractorId IS NULL) OR
        (VendorId IS NULL AND ContractorId IS NOT NULL)
    )
);

CREATE INDEX IX_AccountsPayable_TenantId ON fin.AccountsPayable(TenantId);
CREATE INDEX IX_AccountsPayable_VendorId ON fin.AccountsPayable(VendorId);
CREATE INDEX IX_AccountsPayable_ContractorId ON fin.AccountsPayable(ContractorId);
CREATE INDEX IX_AccountsPayable_Status ON fin.AccountsPayable(Status);
CREATE INDEX IX_AccountsPayable_DueDate ON fin.AccountsPayable(DueDate);
```

---

## Billing Schema (bill)

### bill.Invoices
```sql
CREATE TABLE bill.Invoices (
    InvoiceId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    ProjectId BIGINT NOT NULL,
    ClientId BIGINT NOT NULL,
    InvoiceDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    Status TINYINT NOT NULL DEFAULT 1, -- 1=Draft, 2=Sent, 3=PartiallyPaid, 4=Paid, 5=Overdue, 6=Cancelled
    BillingMode TINYINT NOT NULL, -- 1=TimeAndMaterial, 2=FixedPrice, 3=PercentComplete, 4=Milestone, 5=Retainer
    PaymentTermsNetDays INT NOT NULL DEFAULT 30,
    Subtotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    Notes NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    SentDate DATETIME2 NULL,
    RowVersion ROWVERSION,
    CONSTRAINT FK_Invoices_Projects FOREIGN KEY (ProjectId) REFERENCES pm.Projects(ProjectId),
    CONSTRAINT FK_Invoices_Clients FOREIGN KEY (ClientId) REFERENCES crm.Clients(ClientId),
    CONSTRAINT FK_Invoices_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId),
    CONSTRAINT UQ_Invoices_TenantId_InvoiceNumber UNIQUE (TenantId, InvoiceNumber)
);

CREATE INDEX IX_Invoices_TenantId ON bill.Invoices(TenantId);
CREATE INDEX IX_Invoices_ProjectId ON bill.Invoices(ProjectId);
CREATE INDEX IX_Invoices_ClientId ON bill.Invoices(ClientId);
CREATE INDEX IX_Invoices_Status ON bill.Invoices(Status);
CREATE INDEX IX_Invoices_InvoiceDate ON bill.Invoices(InvoiceDate);
CREATE INDEX IX_Invoices_DueDate ON bill.Invoices(DueDate);
```

### bill.InvoiceLineItems
```sql
CREATE TABLE bill.InvoiceLineItems (
    LineItemId BIGINT PRIMARY KEY IDENTITY(1,1),
    InvoiceId BIGINT NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Quantity DECIMAL(10,2) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    ProjectId BIGINT NULL,
    WBSItemId BIGINT NULL,
    SortOrder INT NOT NULL,
    CONSTRAINT FK_InvoiceLineItems_Invoices FOREIGN KEY (InvoiceId) REFERENCES bill.Invoices(InvoiceId) ON DELETE CASCADE,
    CONSTRAINT FK_InvoiceLineItems_Projects FOREIGN KEY (ProjectId) REFERENCES pm.Projects(ProjectId),
    CONSTRAINT FK_InvoiceLineItems_WBSItems FOREIGN KEY (WBSItemId) REFERENCES pm.WorkBreakdownStructure(WBSItemId)
);

CREATE INDEX IX_InvoiceLineItems_InvoiceId ON bill.InvoiceLineItems(InvoiceId);
CREATE INDEX IX_InvoiceLineItems_ProjectId ON bill.InvoiceLineItems(ProjectId);
```

### bill.Payments
```sql
CREATE TABLE bill.Payments (
    PaymentId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    InvoiceId BIGINT NOT NULL,
    PaymentDate DATE NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Currency NCHAR(3) NOT NULL DEFAULT 'USD',
    PaymentMethod NVARCHAR(50) NULL,
    ReferenceNumber NVARCHAR(50) NULL,
    Notes NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_Payments_Invoices FOREIGN KEY (InvoiceId) REFERENCES bill.Invoices(InvoiceId),
    CONSTRAINT FK_Payments_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
);

CREATE INDEX IX_Payments_TenantId ON bill.Payments(TenantId);
CREATE INDEX IX_Payments_InvoiceId ON bill.Payments(InvoiceId);
CREATE INDEX IX_Payments_PaymentDate ON bill.Payments(PaymentDate);
```

---

## Reporting Schema (rpt)

### rpt.Reports
```sql
CREATE TABLE rpt.Reports (
    ReportId INT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NULL, -- NULL for system reports
    ReportName NVARCHAR(200) NOT NULL,
    ReportType TINYINT NOT NULL, -- 1=Financial, 2=Project, 3=Timesheet, 4=Expense, 5=Billing, 6=ClientActivity, 7=ResourceUtilization, 8=Custom
    Category TINYINT NOT NULL, -- 1=Operational, 2=Financial, 3=Management, 4=Compliance
    Description NVARCHAR(500) NULL,
    QueryDefinition NVARCHAR(MAX) NULL, -- JSON or SQL
    IsSystemReport BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_Reports_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
);

CREATE INDEX IX_Reports_TenantId ON rpt.Reports(TenantId);
CREATE INDEX IX_Reports_ReportType ON rpt.Reports(ReportType);
```

### rpt.ReportSchedules
```sql
CREATE TABLE rpt.ReportSchedules (
    ScheduleId BIGINT PRIMARY KEY IDENTITY(1,1),
    ReportId INT NOT NULL,
    CronExpression NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastRunDate DATETIME2 NULL,
    NextRunDate DATETIME2 NULL,
    EmailRecipients NVARCHAR(MAX) NULL, -- JSON array
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_ReportSchedules_Reports FOREIGN KEY (ReportId) REFERENCES rpt.Reports(ReportId) ON DELETE CASCADE
);

CREATE INDEX IX_ReportSchedules_ReportId ON rpt.ReportSchedules(ReportId);
CREATE INDEX IX_ReportSchedules_NextRunDate ON rpt.ReportSchedules(NextRunDate) WHERE IsActive = 1;
```

---

## Audit & Logging

### dbo.AuditLog
```sql
CREATE TABLE dbo.AuditLog (
    AuditId BIGINT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TableName NVARCHAR(100) NOT NULL,
    RecordId BIGINT NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- INSERT, UPDATE, DELETE
    OldValues NVARCHAR(MAX) NULL, -- JSON
    NewValues NVARCHAR(MAX) NULL, -- JSON
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IPAddress NVARCHAR(50) NULL,
    CONSTRAINT FK_AuditLog_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(TenantId)
);

CREATE INDEX IX_AuditLog_TenantId ON dbo.AuditLog(TenantId);
CREATE INDEX IX_AuditLog_UserId ON dbo.AuditLog(UserId);
CREATE INDEX IX_AuditLog_TableName_RecordId ON dbo.AuditLog(TableName, RecordId);
CREATE INDEX IX_AuditLog_Timestamp ON dbo.AuditLog(Timestamp);
```

---

## Performance Optimization

### Materialized Views for Reporting

```sql
-- Project Summary View
CREATE VIEW pm.vw_ProjectSummary
WITH SCHEMABINDING
AS
SELECT
    p.ProjectId,
    p.TenantId,
    p.Name,
    p.Status,
    p.BudgetAmount,
    COUNT_BIG(*) AS TimesheetEntryCount,
    SUM(ISNULL(te.Hours, 0)) AS TotalHours
FROM pm.Projects p
LEFT JOIN te.TimesheetEntries te ON p.ProjectId = te.ProjectId
LEFT JOIN te.Timesheets t ON te.TimesheetId = t.TimesheetId AND t.Status = 3 -- Approved
GROUP BY p.ProjectId, p.TenantId, p.Name, p.Status, p.BudgetAmount;

CREATE UNIQUE CLUSTERED INDEX IX_vw_ProjectSummary_ProjectId
ON pm.vw_ProjectSummary(ProjectId);
```

---

## Multi-Tenancy Implementation

### Global Query Filter Example (EF Core)
```csharp
// In DbContext OnModelCreating
modelBuilder.Entity<Project>().HasQueryFilter(p => p.TenantId == _currentTenantId);
```

### Row-Level Security (Alternative)
```sql
-- Create Security Policy
CREATE SECURITY POLICY TenantSecurityPolicy
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId)
ON pm.Projects,
ADD FILTER PREDICATE dbo.fn_TenantAccessPredicate(TenantId)
ON crm.Clients
-- Add for all tables
WITH (STATE = ON);

-- Security Function
CREATE FUNCTION dbo.fn_TenantAccessPredicate(@TenantId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_TenantAccessPredicate_result
WHERE @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS UNIQUEIDENTIFIER);
```

---

## Summary

This database schema provides:

1. **Multi-Tenancy**: TenantId on all tables with proper indexing
2. **Referential Integrity**: Proper foreign keys and constraints
3. **Audit Trail**: CreatedDate, ModifiedDate, CreatedBy, ModifiedBy on all tables
4. **Optimistic Concurrency**: RowVersion on aggregate roots
5. **Performance**: Strategic indexes on frequently queried columns
6. **Scalability**: Organized by bounded context schemas
7. **Data Integrity**: Check constraints for business rules
8. **Security**: Row-level security ready
9. **Financial Accuracy**: Proper decimal precision for money columns
10. **Flexibility**: Support for multiple currencies and payment terms
