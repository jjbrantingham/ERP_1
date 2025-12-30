# ERP SaaS Application - Domain Model Design

## Domain-Driven Design Principles

This document outlines the domain models for each bounded context using DDD principles:
- **Entities**: Objects with unique identity that persist over time
- **Value Objects**: Immutable objects defined by their attributes
- **Aggregates**: Cluster of entities and value objects with a root entity
- **Domain Events**: Significant occurrences in the domain
- **Domain Services**: Operations that don't naturally belong to entities

---

## 1. Project Management Bounded Context

### Aggregates

#### Project (Aggregate Root)
```csharp
public class Project : AggregateRoot
{
    public ProjectId Id { get; private set; }
    public string ProjectNumber { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ProjectType Type { get; private set; } // Billable, Overhead, Proposal
    public ProjectStatus Status { get; private set; } // Active, Completed, OnHold, Cancelled
    public ClientId ClientId { get; private set; }
    public Money Budget { get; private set; }
    public DateRange Timeline { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }
    public decimal PercentComplete { get; private set; }

    // Collections (within aggregate boundary)
    private readonly List<WorkBreakdownStructureItem> _wbsItems;
    public IReadOnlyCollection<WorkBreakdownStructureItem> WBSItems => _wbsItems.AsReadOnly();

    private readonly List<ProjectResourceAllocation> _resourceAllocations;
    public IReadOnlyCollection<ProjectResourceAllocation> ResourceAllocations => _resourceAllocations.AsReadOnly();

    // Methods
    public void UpdateProgress(decimal percentComplete);
    public void AddWBSItem(string name, string description, Money budget);
    public void AllocateResource(EmployeeId employeeId, ResourceTypeId resourceTypeId, decimal allocationPercentage);
    public void ChangeStatus(ProjectStatus newStatus);
    public void UpdateTimeline(DateRange newTimeline);
}
```

#### Contract (Entity within Project aggregate or separate aggregate)
```csharp
public class Contract : Entity
{
    public ContractId Id { get; private set; }
    public ProjectId ProjectId { get; private set; }
    public string ContractNumber { get; private set; }
    public ContractType Type { get; private set; } // FixedPrice, TimeAndMaterial, Retainer
    public Money ContractValue { get; private set; }
    public DateRange ContractPeriod { get; private set; }
    public string Terms { get; private set; }
    public ContractStatus Status { get; private set; }
    public DateTime SignedDate { get; private set; }

    private readonly List<ContractMilestone> _milestones;
    public IReadOnlyCollection<ContractMilestone> Milestones => _milestones.AsReadOnly();
}
```

### Entities

#### WorkBreakdownStructureItem
```csharp
public class WorkBreakdownStructureItem : Entity
{
    public WBSItemId Id { get; private set; }
    public string Code { get; private set; } // e.g., "1.2.3"
    public string Name { get; private set; }
    public string Description { get; private set; }
    public WBSItemId? ParentId { get; private set; }
    public Money Budget { get; private set; }
    public int Level { get; private set; }
    public int SortOrder { get; private set; }
}
```

#### ProjectResourceAllocation
```csharp
public class ProjectResourceAllocation : Entity
{
    public AllocationId Id { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public ResourceTypeId ResourceTypeId { get; private set; }
    public decimal AllocationPercentage { get; private set; }
    public DateRange AllocationPeriod { get; private set; }
    public Money BillingRate { get; private set; }
}
```

### Value Objects

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money Add(Money other);
    public Money Subtract(Money other);
    public Money Multiply(decimal factor);
}

public class DateRange : ValueObject
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    public int DurationInDays => (EndDate - StartDate).Days;
    public bool Overlaps(DateRange other);
    public bool Contains(DateTime date);
}
```

### Enums

```csharp
public enum ProjectType
{
    Billable,
    Overhead,
    Proposal
}

public enum ProjectStatus
{
    Draft,
    Active,
    OnHold,
    Completed,
    Cancelled,
    Archived
}

public enum ContractType
{
    FixedPrice,
    TimeAndMaterial,
    Retainer,
    MilestoneBased
}
```

### Domain Events

```csharp
public class ProjectCreatedEvent : DomainEvent
{
    public ProjectId ProjectId { get; }
    public string ProjectName { get; }
}

public class ProjectStatusChangedEvent : DomainEvent
{
    public ProjectId ProjectId { get; }
    public ProjectStatus OldStatus { get; }
    public ProjectStatus NewStatus { get; }
}

public class ProjectCompletedEvent : DomainEvent
{
    public ProjectId ProjectId { get; }
    public DateTime CompletionDate { get; }
}
```

---

## 2. Human Resources Bounded Context

### Aggregates

#### Employee (Aggregate Root)
```csharp
public class Employee : AggregateRoot
{
    public EmployeeId Id { get; private set; }
    public string EmployeeNumber { get; private set; }
    public PersonName Name { get; private set; }
    public Email EmailAddress { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public Address Address { get; private set; }
    public EmploymentType EmploymentType { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public DateTime HireDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }

    private readonly List<EmployeeResourceType> _resourceTypes;
    public IReadOnlyCollection<EmployeeResourceType> ResourceTypes => _resourceTypes.AsReadOnly();

    private readonly List<EmployeeRate> _rates;
    public IReadOnlyCollection<EmployeeRate> Rates => _rates.AsReadOnly();

    // Methods
    public void AssignResourceType(ResourceTypeId resourceTypeId, DateTime effectiveDate);
    public void UpdateRate(Money costRate, Money billingRate, DateTime effectiveDate);
    public void Terminate(DateTime terminationDate);
}
```

#### ResourceType (Aggregate Root)
```csharp
public class ResourceType : AggregateRoot
{
    public ResourceTypeId Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Money DefaultCostRate { get; private set; }
    public Money DefaultBillingRate { get; private set; }
    public bool IsActive { get; private set; }
}
```

### Entities

#### EmployeeResourceType
```csharp
public class EmployeeResourceType : Entity
{
    public Id Id { get; private set; }
    public ResourceTypeId ResourceTypeId { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsPrimary { get; private set; }
}
```

#### EmployeeRate
```csharp
public class EmployeeRate : Entity
{
    public RateId Id { get; private set; }
    public Money CostRate { get; private set; }
    public Money BillingRate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public DateTime? EndDate { get; private set; }
}
```

### Value Objects

```csharp
public class PersonName : ValueObject
{
    public string FirstName { get; }
    public string MiddleName { get; }
    public string LastName { get; }
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();
}

public class Email : ValueObject
{
    public string Value { get; }
    // Validation in constructor
}

public class PhoneNumber : ValueObject
{
    public string Value { get; }
    // Formatting and validation
}

public class Address : ValueObject
{
    public string Street1 { get; }
    public string Street2 { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }
}
```

### Enums

```csharp
public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Consultant
}

public enum EmployeeStatus
{
    Active,
    OnLeave,
    Terminated,
    Retired
}
```

---

## 3. Customer Relationship Management Bounded Context

### Aggregates

#### Client (Aggregate Root)
```csharp
public class Client : AggregateRoot
{
    public ClientId Id { get; private set; }
    public string ClientNumber { get; private set; }
    public string Name { get; private set; }
    public ClientType Type { get; private set; } // Corporation, SmallBusiness, Individual
    public ClientStatus Status { get; private set; }
    public Address BillingAddress { get; private set; }
    public Address ShippingAddress { get; private set; }
    public PaymentTerms PaymentTerms { get; private set; }
    public string TaxId { get; private set; }

    private readonly List<Contact> _contacts;
    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();

    // Methods
    public void AddContact(Contact contact);
    public void UpdateBillingAddress(Address address);
    public void ChangeStatus(ClientStatus newStatus);
}
```

#### Contact (Can be aggregate root or entity depending on design)
```csharp
public class Contact : AggregateRoot
{
    public ContactId Id { get; private set; }
    public PersonName Name { get; private set; }
    public Email EmailAddress { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public PhoneNumber Mobile { get; private set; }
    public string JobTitle { get; private set; }
    public ContactType Type { get; private set; } // Client, Contractor, Vendor
    public bool IsPrimary { get; private set; }

    // Relationships
    public ClientId? ClientId { get; private set; }
    public ContractorId? ContractorId { get; private set; }
    public VendorId? VendorId { get; private set; }

    private readonly List<Note> _notes;
    public IReadOnlyCollection<Note> Notes => _notes.AsReadOnly();
}
```

### Entities

#### Note
```csharp
public class Note : Entity
{
    public NoteId Id { get; private set; }
    public string Subject { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public EmployeeId CreatedBy { get; private set; }
}
```

### Value Objects

```csharp
public class PaymentTerms : ValueObject
{
    public int NetDays { get; }
    public decimal? DiscountPercentage { get; }
    public int? DiscountDays { get; }

    public static PaymentTerms Net30 => new PaymentTerms(30, null, null);
    public static PaymentTerms Net60 => new PaymentTerms(60, null, null);
}
```

### Enums

```csharp
public enum ClientType
{
    Corporation,
    SmallBusiness,
    Individual,
    Government,
    NonProfit
}

public enum ClientStatus
{
    Prospect,
    Active,
    Inactive,
    OnHold
}

public enum ContactType
{
    Client,
    Contractor,
    Vendor,
    Other
}
```

---

## 4. Vendor Management Bounded Context

### Aggregates

#### Contractor (Aggregate Root)
```csharp
public class Contractor : AggregateRoot
{
    public ContractorId Id { get; private set; }
    public string ContractorNumber { get; private set; }
    public string CompanyName { get; private set; }
    public ContractorType Type { get; private set; }
    public ContractorStatus Status { get; private set; }
    public Address Address { get; private set; }
    public Email EmailAddress { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public string TaxId { get; private set; }
    public PaymentTerms PaymentTerms { get; private set; }

    private readonly List<ContractorRate> _rates;
    public IReadOnlyCollection<ContractorRate> Rates => _rates.AsReadOnly();
}
```

#### Vendor (Aggregate Root)
```csharp
public class Vendor : AggregateRoot
{
    public VendorId Id { get; private set; }
    public string VendorNumber { get; private set; }
    public string CompanyName { get; private set; }
    public VendorType Type { get; private set; }
    public VendorStatus Status { get; private set; }
    public Address Address { get; private set; }
    public Email EmailAddress { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public string TaxId { get; private set; }
    public PaymentTerms PaymentTerms { get; private set; }

    private readonly List<VendorCategory> _categories;
    public IReadOnlyCollection<VendorCategory> Categories => _categories.AsReadOnly();
}
```

---

## 5. Time & Expense Bounded Context

### Aggregates

#### Timesheet (Aggregate Root)
```csharp
public class Timesheet : AggregateRoot
{
    public TimesheetId Id { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public DateRange Period { get; private set; } // Typically weekly or bi-weekly
    public TimesheetStatus Status { get; private set; }
    public DateTime? SubmittedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public EmployeeId? ApprovedBy { get; private set; }

    private readonly List<TimesheetEntry> _entries;
    public IReadOnlyCollection<TimesheetEntry> Entries => _entries.AsReadOnly();

    // Methods
    public void AddEntry(ProjectId projectId, WBSItemId? wbsItemId, DateTime date, decimal hours, string description);
    public void Submit();
    public void Approve(EmployeeId approverId);
    public void Reject(EmployeeId rejectedBy, string reason);
    public decimal TotalHours => _entries.Sum(e => e.Hours);
}
```

#### ExpenseReport (Aggregate Root)
```csharp
public class ExpenseReport : AggregateRoot
{
    public ExpenseReportId Id { get; private set; }
    public string ReportNumber { get; private set; }
    public EmployeeId EmployeeId { get; private set; }
    public DateRange Period { get; private set; }
    public ExpenseReportStatus Status { get; private set; }
    public DateTime? SubmittedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public EmployeeId? ApprovedBy { get; private set; }

    private readonly List<ExpenseEntry> _entries;
    public IReadOnlyCollection<ExpenseEntry> Entries => _entries.AsReadOnly();

    public Money TotalAmount => new Money(
        _entries.Sum(e => e.Amount.Amount),
        _entries.FirstOrDefault()?.Amount.Currency ?? "USD"
    );

    // Methods
    public void AddEntry(ProjectId projectId, ExpenseCategory category, DateTime date, Money amount, string description);
    public void Submit();
    public void Approve(EmployeeId approverId);
    public void Reject(EmployeeId rejectedBy, string reason);
}
```

### Entities

#### TimesheetEntry
```csharp
public class TimesheetEntry : Entity
{
    public EntryId Id { get; private set; }
    public ProjectId ProjectId { get; private set; }
    public WBSItemId? WBSItemId { get; private set; }
    public DateTime Date { get; private set; }
    public decimal Hours { get; private set; }
    public string Description { get; private set; }
    public bool IsBillable { get; private set; }
}
```

#### ExpenseEntry
```csharp
public class ExpenseEntry : Entity
{
    public EntryId Id { get; private set; }
    public ProjectId ProjectId { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public DateTime Date { get; private set; }
    public Money Amount { get; private set; }
    public string Description { get; private set; }
    public string ReceiptUrl { get; private set; } // Azure Blob Storage URL
    public bool IsBillable { get; private set; }
    public bool IsReimbursable { get; private set; }
}
```

### Enums

```csharp
public enum TimesheetStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected
}

public enum ExpenseReportStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
    Paid
}

public enum ExpenseCategory
{
    Travel,
    Meals,
    Lodging,
    Transportation,
    Supplies,
    Equipment,
    Other
}
```

---

## 6. Financial Management Bounded Context

### Aggregates

#### ChartOfAccounts (Aggregate Root)
```csharp
public class ChartOfAccounts : AggregateRoot
{
    public ChartOfAccountsId Id { get; private set; }

    private readonly List<Account> _accounts;
    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    public void AddAccount(string accountNumber, string name, AccountType type, AccountCategory category);
}
```

#### Account (Entity within ChartOfAccounts or separate aggregate)
```csharp
public class Account : Entity
{
    public AccountId Id { get; private set; }
    public string AccountNumber { get; private set; }
    public string Name { get; private set; }
    public AccountType Type { get; private set; } // Asset, Liability, Equity, Revenue, Expense
    public AccountCategory Category { get; private set; }
    public AccountId? ParentAccountId { get; private set; }
    public bool IsActive { get; private set; }
    public Money Balance { get; private set; }
}
```

#### GeneralLedger (Aggregate Root)
```csharp
public class GeneralLedger : AggregateRoot
{
    public LedgerId Id { get; private set; }

    private readonly List<JournalEntry> _journalEntries;
    public IReadOnlyCollection<JournalEntry> JournalEntries => _journalEntries.AsReadOnly();

    public void PostJournalEntry(JournalEntry entry);
}
```

#### JournalEntry (Aggregate Root)
```csharp
public class JournalEntry : AggregateRoot
{
    public JournalEntryId Id { get; private set; }
    public string EntryNumber { get; private set; }
    public DateTime EntryDate { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string Description { get; private set; }
    public JournalEntryType Type { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    public string ReferenceNumber { get; private set; }

    private readonly List<JournalEntryLine> _lines;
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    public bool IsBalanced => _lines.Sum(l => l.DebitAmount.Amount) == _lines.Sum(l => l.CreditAmount.Amount);

    public void AddLine(AccountId accountId, Money debitAmount, Money creditAmount, string description);
    public void Post();
    public void Reverse();
}
```

### Entities

#### JournalEntryLine
```csharp
public class JournalEntryLine : Entity
{
    public LineId Id { get; private set; }
    public AccountId AccountId { get; private set; }
    public Money DebitAmount { get; private set; }
    public Money CreditAmount { get; private set; }
    public string Description { get; private set; }
    public ProjectId? ProjectId { get; private set; }
}
```

#### AccountsReceivable (Aggregate Root)
```csharp
public class AccountsReceivable : AggregateRoot
{
    public ARId Id { get; private set; }
    public ClientId ClientId { get; private set; }
    public InvoiceId InvoiceId { get; private set; }
    public Money Amount { get; private set; }
    public Money AmountPaid { get; private set; }
    public Money Balance => Amount.Subtract(AmountPaid);
    public DateTime DueDate { get; private set; }
    public ARStatus Status { get; private set; }
}
```

#### AccountsPayable (Aggregate Root)
```csharp
public class AccountsPayable : AggregateRoot
{
    public APId Id { get; private set; }
    public VendorId? VendorId { get; private set; }
    public ContractorId? ContractorId { get; private set; }
    public string ReferenceNumber { get; private set; }
    public Money Amount { get; private set; }
    public Money AmountPaid { get; private set; }
    public Money Balance => Amount.Subtract(AmountPaid);
    public DateTime DueDate { get; private set; }
    public APStatus Status { get; private set; }
}
```

### Enums

```csharp
public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense
}

public enum AccountCategory
{
    CurrentAsset,
    FixedAsset,
    CurrentLiability,
    LongTermLiability,
    Equity,
    OperatingRevenue,
    OtherRevenue,
    CostOfGoodsSold,
    OperatingExpense,
    OtherExpense
}

public enum JournalEntryType
{
    Standard,
    Adjusting,
    Closing,
    Reversing
}

public enum JournalEntryStatus
{
    Draft,
    Posted,
    Reversed
}
```

---

## 7. Billing Bounded Context

### Aggregates

#### Invoice (Aggregate Root)
```csharp
public class Invoice : AggregateRoot
{
    public InvoiceId Id { get; private set; }
    public string InvoiceNumber { get; private set; }
    public ProjectId ProjectId { get; private set; }
    public ClientId ClientId { get; private set; }
    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public BillingMode BillingMode { get; private set; }
    public PaymentTerms PaymentTerms { get; private set; }

    private readonly List<InvoiceLineItem> _lineItems;
    public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    public Money Subtotal => new Money(
        _lineItems.Sum(li => li.Amount.Amount),
        "USD"
    );
    public Money TaxAmount { get; private set; }
    public Money Total => Subtotal.Add(TaxAmount);

    // Methods
    public void AddLineItem(string description, decimal quantity, Money unitPrice);
    public void CalculateTax(decimal taxRate);
    public void Send();
    public void MarkAsPaid(DateTime paymentDate, Money amount);
    public void Cancel();
}
```

### Entities

#### InvoiceLineItem
```csharp
public class InvoiceLineItem : Entity
{
    public LineItemId Id { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money Amount => UnitPrice.Multiply(Quantity);
    public ProjectId? ProjectId { get; private set; }
    public WBSItemId? WBSItemId { get; private set; }
}
```

### Value Objects

```csharp
public class BillingMode : ValueObject
{
    public BillingModeType Type { get; }
    public decimal? Percentage { get; } // For PercentComplete

    public static BillingMode TimeAndMaterial => new BillingMode(BillingModeType.TimeAndMaterial, null);
    public static BillingMode FixedPrice => new BillingMode(BillingModeType.FixedPrice, null);
    public static BillingMode PercentComplete(decimal percentage) =>
        new BillingMode(BillingModeType.PercentComplete, percentage);
}
```

### Enums

```csharp
public enum InvoiceStatus
{
    Draft,
    Sent,
    PartiallyPaid,
    Paid,
    Overdue,
    Cancelled
}

public enum BillingModeType
{
    TimeAndMaterial,
    FixedPrice,
    PercentComplete,
    Milestone,
    Retainer
}
```

### Domain Services

```csharp
public interface IInvoiceGenerationService
{
    Invoice GenerateInvoiceForProject(ProjectId projectId, DateRange period);
    Invoice GenerateInvoiceByPercentComplete(ProjectId projectId, decimal percentComplete);
    Invoice GenerateInvoiceByMilestone(ProjectId projectId, ContractMilestoneId milestoneId);
}
```

---

## 8. Reporting Bounded Context

### Aggregates

#### Report (Aggregate Root)
```csharp
public class Report : AggregateRoot
{
    public ReportId Id { get; private set; }
    public string ReportName { get; private set; }
    public ReportType Type { get; private set; }
    public ReportCategory Category { get; private set; }
    public string Description { get; private set; }
    public string QueryDefinition { get; private set; } // JSON or SQL
    public bool IsSystemReport { get; private set; }

    private readonly List<ReportParameter> _parameters;
    public IReadOnlyCollection<ReportParameter> Parameters => _parameters.AsReadOnly();
}
```

#### ReportSchedule (Aggregate Root)
```csharp
public class ReportSchedule : AggregateRoot
{
    public ScheduleId Id { get; private set; }
    public ReportId ReportId { get; private set; }
    public string CronExpression { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastRunDate { get; private set; }
    public DateTime? NextRunDate { get; private set; }

    private readonly List<string> _emailRecipients;
    public IReadOnlyCollection<string> EmailRecipients => _emailRecipients.AsReadOnly();
}
```

### Enums

```csharp
public enum ReportType
{
    Financial,
    Project,
    Timesheet,
    Expense,
    Billing,
    ClientActivity,
    ResourceUtilization,
    Custom
}

public enum ReportCategory
{
    Operational,
    Financial,
    Management,
    Compliance
}
```

---

## Cross-Cutting Concerns

### Base Classes

```csharp
public abstract class Entity
{
    public long Id { get; protected set; }
    public DateTime CreatedDate { get; protected set; }
    public string CreatedBy { get; protected set; }
    public DateTime? ModifiedDate { get; protected set; }
    public string ModifiedBy { get; protected set; }

    private List<DomainEvent> _domainEvents;
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents?.AsReadOnly();

    public void AddDomainEvent(DomainEvent eventItem);
    public void RemoveDomainEvent(DomainEvent eventItem);
    public void ClearDomainEvents();
}

public abstract class AggregateRoot : Entity, IAggregateRoot
{
    public Guid TenantId { get; protected set; }
    public byte[] RowVersion { get; protected set; } // For optimistic concurrency
}

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj);
    public override int GetHashCode();
}
```

### Multi-Tenancy

Every aggregate root includes:
```csharp
public Guid TenantId { get; protected set; }
```

This ensures data isolation at the entity level with global query filters in EF Core.

---

## Relationships Between Bounded Contexts

### Integration Events (for eventual consistency)

```csharp
// When a project is created
public class ProjectCreatedIntegrationEvent
{
    public Guid ProjectId { get; set; }
    public Guid ClientId { get; set; }
    public string ProjectName { get; set; }
}

// When timesheet is approved (Financial context needs to know)
public class TimesheetApprovedIntegrationEvent
{
    public Guid TimesheetId { get; set; }
    public Guid EmployeeId { get; set; }
    public List<TimesheetEntryDto> Entries { get; set; }
}

// When invoice is created (Financial context needs to create AR)
public class InvoiceCreatedIntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid ClientId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime DueDate { get; set; }
}
```

---

## Summary

This domain model provides:

1. **Clear Boundaries**: Each bounded context has well-defined aggregates
2. **Consistency**: Aggregate boundaries ensure transactional consistency
3. **Encapsulation**: Business logic is encapsulated in domain entities
4. **Rich Domain Model**: Entities have behavior, not just data
5. **Immutability**: Value objects are immutable
6. **Domain Events**: Enable reactive programming and eventual consistency
7. **Multi-Tenancy**: Built-in tenant isolation
8. **Audit Trail**: Created/Modified tracking on all entities

The design follows DDD tactical patterns and prepares the application for:
- Scalability through clear boundaries
- Maintainability through separation of concerns
- Testability through rich domain models
- Extensibility through domain events
