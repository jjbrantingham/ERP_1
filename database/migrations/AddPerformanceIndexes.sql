-- =============================================
-- Performance Optimization Indexes
-- Phase 12: Polish & Optimization
-- =============================================

USE [ERP_Database];
GO

PRINT 'Adding performance indexes...';

-- =============================================
-- IDENTITY Schema Indexes
-- =============================================

-- Users: Optimize login queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('identity.Users'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_Email ON identity.Users(Email) INCLUDE (PasswordHash, IsActive);
    PRINT 'Created index: IX_Users_Email';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_TenantId_IsActive' AND object_id = OBJECT_ID('identity.Users'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_TenantId_IsActive ON identity.Users(TenantId, IsActive) INCLUDE (Email, FirstName, LastName);
    PRINT 'Created index: IX_Users_TenantId_IsActive';
END

-- =============================================
-- PM Schema Indexes
-- =============================================

-- Projects: Optimize status and client queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Projects_ClientId_Status' AND object_id = OBJECT_ID('pm.Projects'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Projects_ClientId_Status ON pm.Projects(ClientId, Status) INCLUDE (Name, StartDate, EndDate);
    PRINT 'Created index: IX_Projects_ClientId_Status';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Projects_StartDate_EndDate' AND object_id = OBJECT_ID('pm.Projects'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Projects_StartDate_EndDate ON pm.Projects(StartDate, EndDate) WHERE Status IN (1, 2); -- Active and OnHold
    PRINT 'Created index: IX_Projects_StartDate_EndDate';
END

-- WBS Items: Optimize hierarchy queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WBSItems_ParentId' AND object_id = OBJECT_ID('pm.WBSItems'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WBSItems_ParentId ON pm.WBSItems(ParentId) INCLUDE (Name, SequenceNumber);
    PRINT 'Created index: IX_WBSItems_ParentId';
END

-- =============================================
-- TE Schema Indexes
-- =============================================

-- Timesheets: Optimize employee and date range queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Timesheets_EmployeeId_WeekStart_Status' AND object_id = OBJECT_ID('te.Timesheets'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Timesheets_EmployeeId_WeekStart_Status ON te.Timesheets(EmployeeId, WeekStartDate, Status);
    PRINT 'Created index: IX_Timesheets_EmployeeId_WeekStart_Status';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Timesheets_Status_WeekStart' AND object_id = OBJECT_ID('te.Timesheets'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Timesheets_Status_WeekStart ON te.Timesheets(Status, WeekStartDate) INCLUDE (EmployeeId, TotalHours);
    PRINT 'Created index: IX_Timesheets_Status_WeekStart';
END

-- Timesheet Entries: Optimize project queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TimesheetEntries_ProjectId_Date' AND object_id = OBJECT_ID('te.TimesheetEntries'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TimesheetEntries_ProjectId_Date ON te.TimesheetEntries(ProjectId, EntryDate) INCLUDE (Hours, Description);
    PRINT 'Created index: IX_TimesheetEntries_ProjectId_Date';
END

-- Expense Reports: Optimize employee and status queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ExpenseReports_EmployeeId_Date_Status' AND object_id = OBJECT_ID('te.ExpenseReports'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ExpenseReports_EmployeeId_Date_Status ON te.ExpenseReports(EmployeeId, ReportDate, Status);
    PRINT 'Created index: IX_ExpenseReports_EmployeeId_Date_Status';
END

-- =============================================
-- FIN Schema Indexes
-- =============================================

-- Journal Entries: Optimize posting and date queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JournalEntries_PostedDate_Status' AND object_id = OBJECT_ID('fin.JournalEntries'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_JournalEntries_PostedDate_Status ON fin.JournalEntries(PostedDate, Status) WHERE PostedDate IS NOT NULL;
    PRINT 'Created index: IX_JournalEntries_PostedDate_Status';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JournalEntries_EntryDate' AND object_id = OBJECT_ID('fin.JournalEntries'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_JournalEntries_EntryDate ON fin.JournalEntries(EntryDate DESC);
    PRINT 'Created index: IX_JournalEntries_EntryDate';
END

-- Journal Entry Lines: Optimize account queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_JournalEntryLines_AccountId' AND object_id = OBJECT_ID('fin.JournalEntryLines'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_JournalEntryLines_AccountId ON fin.JournalEntryLines(AccountId) INCLUDE (DebitAmount, CreditAmount);
    PRINT 'Created index: IX_JournalEntryLines_AccountId';
END

-- Accounts: Optimize account type and code queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Accounts_AccountType_IsActive' AND object_id = OBJECT_ID('fin.Accounts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Accounts_AccountType_IsActive ON fin.Accounts(AccountType, IsActive) INCLUDE (AccountCode, Name);
    PRINT 'Created index: IX_Accounts_AccountType_IsActive';
END

-- =============================================
-- BILL Schema Indexes
-- =============================================

-- Invoices: Optimize client and status queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_ClientId_Status' AND object_id = OBJECT_ID('bill.Invoices'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Invoices_ClientId_Status ON bill.Invoices(ClientId, Status) INCLUDE (InvoiceDate, DueDate, TotalAmount);
    PRINT 'Created index: IX_Invoices_ClientId_Status';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_DueDate_Status' AND object_id = OBJECT_ID('bill.Invoices'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Invoices_DueDate_Status ON bill.Invoices(DueDate, Status) WHERE Status IN (2, 3, 4); -- Posted, PartiallyPaid, Overdue
    PRINT 'Created index: IX_Invoices_DueDate_Status';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_ProjectId' AND object_id = OBJECT_ID('bill.Invoices'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Invoices_ProjectId ON bill.Invoices(ProjectId) WHERE ProjectId IS NOT NULL;
    PRINT 'Created index: IX_Invoices_ProjectId';
END

-- Payments: Optimize invoice and date queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Payments_InvoiceId' AND object_id = OBJECT_ID('bill.Payments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Payments_InvoiceId ON bill.Payments(InvoiceId) INCLUDE (PaymentDate, Amount);
    PRINT 'Created index: IX_Payments_InvoiceId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Payments_PaymentDate' AND object_id = OBJECT_ID('bill.Payments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Payments_PaymentDate ON bill.Payments(PaymentDate DESC);
    PRINT 'Created index: IX_Payments_PaymentDate';
END

-- =============================================
-- WF Schema Indexes
-- =============================================

-- Workflow Instances: Optimize status and entity queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WorkflowInstances_Status_StartedDate' AND object_id = OBJECT_ID('wf.WorkflowInstances'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WorkflowInstances_Status_StartedDate ON wf.WorkflowInstances(Status, StartedDate DESC);
    PRINT 'Created index: IX_WorkflowInstances_Status_StartedDate';
END

-- Step Instances: Optimize pending approval queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StepInstances_Status_ApproverId' AND object_id = OBJECT_ID('wf.StepInstances'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_StepInstances_Status_ApproverId ON wf.StepInstances(Status, ApproverId) WHERE ApproverId IS NOT NULL;
    PRINT 'Created index: IX_StepInstances_Status_ApproverId';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StepInstances_Status_ApproverRole' AND object_id = OBJECT_ID('wf.StepInstances'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_StepInstances_Status_ApproverRole ON wf.StepInstances(Status, ApproverRole) WHERE ApproverRole IS NOT NULL;
    PRINT 'Created index: IX_StepInstances_Status_ApproverRole';
END

-- =============================================
-- CRM Schema Indexes
-- =============================================

-- Clients: Optimize search and account manager queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clients_Status_Name' AND object_id = OBJECT_ID('crm.Clients'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Clients_Status_Name ON crm.Clients(Status, Name) INCLUDE (ClientNumber, Email);
    PRINT 'Created index: IX_Clients_Status_Name';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clients_AccountManagerId' AND object_id = OBJECT_ID('crm.Clients'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Clients_AccountManagerId ON crm.Clients(AccountManagerId) WHERE AccountManagerId IS NOT NULL;
    PRINT 'Created index: IX_Clients_AccountManagerId';
END

-- Contacts: Optimize client queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Contacts_ClientId_IsPrimary' AND object_id = OBJECT_ID('crm.Contacts'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Contacts_ClientId_IsPrimary ON crm.Contacts(ClientId, IsPrimary) INCLUDE (FirstName, LastName, Email);
    PRINT 'Created index: IX_Contacts_ClientId_IsPrimary';
END

-- =============================================
-- HR Schema Indexes
-- =============================================

-- Employees: Optimize department and status queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_Department_Status' AND object_id = OBJECT_ID('hr.Employees'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Employees_Department_Status ON hr.Employees(Department, EmploymentStatus) INCLUDE (FirstName, LastName, Email);
    PRINT 'Created index: IX_Employees_Department_Status';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employees_ManagerId' AND object_id = OBJECT_ID('hr.Employees'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Employees_ManagerId ON hr.Employees(ManagerId) WHERE ManagerId IS NOT NULL;
    PRINT 'Created index: IX_Employees_ManagerId';
END

-- Rates: Optimize employee and date queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Rates_EmployeeId_EffectiveDate' AND object_id = OBJECT_ID('hr.Rates'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Rates_EmployeeId_EffectiveDate ON hr.Rates(EmployeeId, EffectiveDate DESC);
    PRINT 'Created index: IX_Rates_EmployeeId_EffectiveDate';
END

-- =============================================
-- Update Statistics
-- =============================================

PRINT 'Updating statistics on all tables...';

-- Update statistics for better query plans
UPDATE STATISTICS identity.Users WITH FULLSCAN;
UPDATE STATISTICS pm.Projects WITH FULLSCAN;
UPDATE STATISTICS te.Timesheets WITH FULLSCAN;
UPDATE STATISTICS te.TimesheetEntries WITH FULLSCAN;
UPDATE STATISTICS fin.JournalEntries WITH FULLSCAN;
UPDATE STATISTICS fin.JournalEntryLines WITH FULLSCAN;
UPDATE STATISTICS bill.Invoices WITH FULLSCAN;
UPDATE STATISTICS bill.Payments WITH FULLSCAN;
UPDATE STATISTICS wf.WorkflowInstances WITH FULLSCAN;
UPDATE STATISTICS wf.StepInstances WITH FULLSCAN;
UPDATE STATISTICS crm.Clients WITH FULLSCAN;
UPDATE STATISTICS hr.Employees WITH FULLSCAN;

PRINT 'Performance indexes added successfully!';
GO
