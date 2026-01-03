# Validate Business Rules Skill

## Purpose
Review implementation of business rules for correctness and completeness.

## Business Rule Patterns

### Invariants in Aggregates
```csharp
public class Invoice : AggregateRoot
{
    public void AddLine(InvoiceLine line)
    {
        // Business rule: Cannot modify posted invoice
        if (Status == InvoiceStatus.Posted)
            throw new InvalidOperationException("Cannot modify posted invoice");

        // Business rule: Line amount must be positive
        if (line.Amount <= 0)
            throw new ArgumentException("Amount must be positive");

        _lines.Add(line);
    }
}
```

### State Transitions
```csharp
public void Post()
{
    // Business rule: Valid state transition
    if (Status != InvoiceStatus.Draft)
        throw new InvalidOperationException($"Cannot post invoice in {Status} status");

    // Business rule: Must have lines
    if (!Lines.Any())
        throw new InvalidOperationException("Cannot post empty invoice");

    Status = InvoiceStatus.Posted;
    PostedDate = DateTime.UtcNow;
}
```

### Financial Calculations
```csharp
// Business rule: Double-entry bookkeeping
var totalDebits = Lines.Sum(l => l.DebitAmount);
var totalCredits = Lines.Sum(l => l.CreditAmount);

if (totalDebits != totalCredits)
    throw new InvalidOperationException("Debits must equal credits");
```

## Related Skills
- financial-accuracy-check
- domain-model-design
