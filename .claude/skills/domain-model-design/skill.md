# Domain Model Design Skill

## Purpose
Design domain models following Domain-Driven Design (DDD) patterns for the ERP SaaS application.

## Design Process

### 1. Identify the Bounded Context
- Which bounded context does this model belong to?
  - Project Management (pm)
  - Human Resources (hr)
  - CRM (crm)
  - Vendor Management (vm)
  - Time & Expense (te)
  - Financial Management (fin)
  - Billing (bill)
  - Reporting (rpt)

### 2. Define Aggregates
- What is the aggregate root?
- What entities belong to this aggregate?
- What are the aggregate boundaries?
- What invariants must be maintained?

### 3. Design Entities
- What is the entity's identity?
- What behavior does it have?
- How does it enforce business rules?
- What domain events should it raise?

### 4. Design Value Objects
- What concepts should be value objects?
- Are they immutable?
- Do they have no identity?
- Do they implement value equality?

### 5. Define Domain Events
- What significant state changes occur?
- What other aggregates/systems need to know?
- What data should events contain?

## Design Checklist

### Aggregate Root
- [ ] Inherits from `AggregateRoot` base class
- [ ] Has strong-typed ID (e.g., `ProjectId`)
- [ ] Enforces all aggregate invariants
- [ ] All setters are private
- [ ] State changes through methods, not property setters
- [ ] Raises domain events for significant changes
- [ ] Controls access to child entities

### Entities
- [ ] Inherit from `Entity` base class
- [ ] Have clear identity
- [ ] Encapsulate behavior
- [ ] Private setters for properties
- [ ] Enforce local invariants

### Value Objects
- [ ] Inherit from `ValueObject` base class
- [ ] Are immutable (all properties readonly)
- [ ] No identity (ID property)
- [ ] Implement equality based on all properties
- [ ] Override `GetEqualityComponents()`
- [ ] Have validation in constructor

### Domain Events
- [ ] Inherit from `DomainEvent` base class
- [ ] Named in past tense (e.g., `InvoicePostedEvent`)
- [ ] Include all relevant data
- [ ] Are serializable
- [ ] Include timestamp

## Example Templates

### Aggregate Root Template
```csharp
public class [AggregateName] : AggregateRoot
{
    public [AggregateName]Id Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string [Property] { get; private set; }

    private readonly List<[ChildEntity]> _[children];
    public IReadOnlyCollection<[ChildEntity]> [Children] => _[children].AsReadOnly();

    // Private constructor for EF Core
    private [AggregateName]() { }

    // Factory method
    public static [AggregateName] Create(
        Guid tenantId,
        string [property])
    {
        var aggregate = new [AggregateName]
        {
            Id = new [AggregateName]Id(0),
            TenantId = tenantId,
            [Property] = [property] ?? throw new ArgumentNullException(nameof([property]))
        };

        aggregate.AddDomainEvent(new [AggregateName]CreatedEvent(aggregate.Id, tenantId));
        return aggregate;
    }

    // Business methods
    public void [DoSomething]([Parameters])
    {
        // Validate
        if (![ValidCondition])
            throw new InvalidOperationException("[Error message]");

        // Change state
        [Property] = [newValue];

        // Raise event
        AddDomainEvent(new [Something]Event(Id, [data]));
    }
}
```

### Value Object Template
```csharp
public class [ValueObjectName] : ValueObject
{
    public [Type] [Property1] { get; }
    public [Type] [Property2] { get; }

    public [ValueObjectName]([Type] [property1], [Type] [property2])
    {
        // Validation
        if ([invalidCondition])
            throw new ArgumentException("[Error]", nameof([property1]));

        [Property1] = [property1];
        [Property2] = [property2];
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return [Property1];
        yield return [Property2];
    }

    // Business methods
    public [ValueObjectName] [Method]([Parameters])
    {
        // Return new instance (immutable)
        return new [ValueObjectName]([calculation]);
    }
}
```

### Domain Event Template
```csharp
public class [Aggregate][Action]Event : DomainEvent
{
    public [AggregateName]Id [Aggregate]Id { get; }
    public [Type] [RelevantData] { get; }
    public DateTime [Action]At { get; }

    public [Aggregate][Action]Event(
        [AggregateName]Id [aggregate]Id,
        [Type] [relevantData])
    {
        [Aggregate]Id = [aggregate]Id;
        [RelevantData] = [relevantData];
        [Action]At = DateTime.UtcNow;
    }
}
```

## Design Questions to Ask

1. **What is the lifecycle of this aggregate?**
   - How is it created?
   - What states can it be in?
   - How does it transition between states?
   - When is it archived/deleted?

2. **What invariants must always be true?**
   - What business rules cannot be violated?
   - What relationships must be maintained?
   - What calculations must always balance?

3. **What are the aggregate boundaries?**
   - What entities should be inside the aggregate?
   - What relationships cross aggregate boundaries?
   - Should this be multiple aggregates?

4. **What events are significant?**
   - What changes need to notify other parts of the system?
   - What needs to be audited?
   - What triggers workflows?

5. **What value objects make sense?**
   - What concepts have no identity?
   - What should be immutable?
   - What has value equality?

## Output Format

Provide a complete domain model design including:

### 1. Context Analysis
- Bounded context
- Related aggregates
- Context map

### 2. Aggregate Design
- Aggregate root class
- Child entities
- Value objects
- Invariants

### 3. Domain Events
- Event definitions
- When they're raised
- Who subscribes

### 4. Repository Interface
- Query methods needed
- Specifications

### 5. EF Core Configuration
- Table mappings
- Relationships
- Indexes

### 6. Test Scenarios
- Key test cases
- Edge cases
- Invariant tests

## Example Usage

```
User: /domain-model-design "Design a Subscription billing model"

Claude: I'll design a Subscription aggregate for the Billing context...

[Provides complete domain model with code]
```

## Related Skills
- architecture-review
- create-aggregate
- validate-business-rules
