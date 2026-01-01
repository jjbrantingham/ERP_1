# Financial Accuracy Check

Verify financial calculations, accounting rules, and data integrity for financial modules.

## What This Skill Does

Performs comprehensive verification of financial logic including:

1. **Decimal Precision**: All money amounts use correct decimal types
2. **Rounding**: Proper rounding rules applied
3. **Double-Entry Bookkeeping**: Debits = Credits
4. **Account Balances**: Balance calculations are correct
5. **Currency Handling**: Currency consistency
6. **Audit Trail**: All financial changes are logged
7. **Immutability**: Posted transactions cannot be modified

## Verification Checklist

### 1. Decimal Types and Precision

✅ **All money amounts use `decimal` type, NEVER `float` or `double`**:

```csharp
// ✅ CORRECT
public decimal Amount { get; private set; }
public Money TotalAmount { get; private set; }  // Value object

// ❌ WRONG
public double Amount { get; private set; }  // NEVER use double for money!
public float Amount { get; private set; }   // NEVER use float for money!
```

✅ **Database columns use appropriate precision**:

```csharp
builder.Property(x => x.Amount)
    .HasPrecision(18, 2);  // 18 total digits, 2 decimal places

// For Money value object:
builder.OwnsOne(x => x.TotalAmount, money =>
{
    money.Property(m => m.Amount)
        .HasColumnName("TotalAmount")
        .HasPrecision(18, 2);  // Consistent precision
    money.Property(m => m.Currency)
        .HasColumnName("Currency")
        .HasMaxLength(3);  // ISO 4217 codes
});
```

### 2. Rounding Rules

✅ **Consistent rounding throughout**:

```csharp
// Use MidpointRounding.AwayFromZero for financial calculations
public decimal CalculateTotal()
{
    var total = _lineItems.Sum(x => x.Quantity * x.UnitPrice);
    return Math.Round(total, 2, MidpointRounding.AwayFromZero);
}

// Document rounding rules
/// <summary>
/// Calculates line item total.
/// Rounds to 2 decimal places using AwayFromZero (banker's rounding).
/// </summary>
public decimal CalculateLineTotal()
{
    return Math.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
}
```

✅ **Round at appropriate times**:

```csharp
// GOOD: Round after all calculations
var subtotal = _lineItems.Sum(x => x.Quantity * x.UnitPrice);
var tax = subtotal * taxRate;
var total = Math.Round(subtotal + tax, 2, MidpointRounding.AwayFromZero);

// AVOID: Rounding intermediate values can compound errors
var subtotal = Math.Round(_lineItems.Sum(x => Math.Round(x.Quantity * x.UnitPrice, 2)), 2);
```

### 3. Double-Entry Bookkeeping

✅ **Every transaction creates balanced journal entries**:

```csharp
public class JournalEntry : AggregateRoot
{
    private readonly List<JournalEntryLine> _lines;

    public void AddLine(long accountId, decimal debitAmount, decimal creditAmount, string description)
    {
        _lines.Add(new JournalEntryLine(accountId, debitAmount, creditAmount, description));
    }

    public void Post()
    {
        // Verify balanced before posting
        if (!IsBalanced())
            throw new InvalidOperationException("Journal entry is not balanced. Debits must equal credits.");

        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Can only post draft journal entries.");

        Status = JournalEntryStatus.Posted;
        PostedDate = DateTime.UtcNow;

        AddDomainEvent(new JournalEntryPostedEvent(Id, TenantId, CalculateDebitTotal()));
    }

    private bool IsBalanced()
    {
        var totalDebits = CalculateDebitTotal();
        var totalCredits = CalculateCreditTotal();

        // Must be exact match (no tolerance for rounding errors)
        return totalDebits == totalCredits;
    }

    public decimal CalculateDebitTotal() =>
        Math.Round(_lines.Sum(x => x.DebitAmount), 2, MidpointRounding.AwayFromZero);

    public decimal CalculateCreditTotal() =>
        Math.Round(_lines.Sum(x => x.CreditAmount), 2, MidpointRounding.AwayFromZero);
}
```

✅ **Verify balance in tests**:

```csharp
[Fact]
public void Post_UnbalancedEntry_ShouldThrowException()
{
    // Arrange
    var entry = JournalEntry.Create(Guid.NewGuid(), "Test Entry", DateTime.UtcNow);
    entry.AddLine(1001, 100.00m, 0, "Debit"); // Debit: $100
    entry.AddLine(2001, 0, 50.00m, "Credit"); // Credit: $50
    // Unbalanced: $100 ≠ $50

    // Act & Assert
    entry.Invoking(e => e.Post())
        .Should().Throw<InvalidOperationException>()
        .WithMessage("*not balanced*");
}

[Fact]
public void Post_BalancedEntry_ShouldSucceed()
{
    // Arrange
    var entry = JournalEntry.Create(Guid.NewGuid(), "Test Entry", DateTime.UtcNow);
    entry.AddLine(1001, 100.00m, 0, "Accounts Receivable"); // Debit
    entry.AddLine(4001, 0, 100.00m, "Revenue");             // Credit
    // Balanced: $100 = $100

    // Act
    entry.Post();

    // Assert
    entry.Status.Should().Be(JournalEntryStatus.Posted);
    entry.PostedDate.Should().NotBeNull();
}
```

### 4. Account Balance Calculations

✅ **Account balances calculated correctly**:

```csharp
public class Account : AggregateRoot
{
    public AccountType Type { get; private set; }
    public decimal Balance { get; private set; }

    public void ApplyTransaction(decimal debitAmount, decimal creditAmount)
    {
        // Asset and Expense accounts: Debit increases, Credit decreases
        // Liability, Equity, Revenue accounts: Credit increases, Debit decreases

        if (Type == AccountType.Asset || Type == AccountType.Expense)
        {
            Balance += debitAmount - creditAmount;
        }
        else if (Type == AccountType.Liability ||
                 Type == AccountType.Equity ||
                 Type == AccountType.Revenue)
        {
            Balance += creditAmount - debitAmount;
        }

        Balance = Math.Round(Balance, 2, MidpointRounding.AwayFromZero);
    }

    public decimal GetBalance(DateTime? asOfDate = null)
    {
        // If asOfDate specified, calculate historical balance
        if (asOfDate.HasValue)
        {
            // Query transactions up to that date
            return CalculateHistoricalBalance(asOfDate.Value);
        }

        return Balance;
    }
}
```

✅ **Balance verification**:

```csharp
public class TrialBalance
{
    public decimal TotalDebits { get; private set; }
    public decimal TotalCredits { get; private set; }

    public bool IsBalanced() => TotalDebits == TotalCredits;

    public decimal GetVariance() => Math.Abs(TotalDebits - TotalCredits);
}
```

### 5. Currency Handling

✅ **Use Money value object for currency support**:

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }  // ISO 4217 (USD, EUR, GBP, etc.)

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));
        if (currency.Length != 3)
            throw new ArgumentException("Currency must be 3-letter ISO code", nameof(currency));

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

✅ **Validate currency consistency**:

```csharp
public void AddLineItem(Money amount, ...)
{
    // Ensure all line items use the same currency as the invoice
    if (_lineItems.Any() && amount.Currency != _lineItems.First().Amount.Currency)
        throw new InvalidOperationException("All line items must use the same currency");

    _lineItems.Add(new InvoiceLineItem(amount, ...));
}
```

### 6. Audit Trail

✅ **All financial changes are logged**:

```csharp
public class FinancialAuditEntry : Entity
{
    public Guid TenantId { get; private set; }
    public string EntityType { get; private set; }  // "Invoice", "Payment", etc.
    public long EntityId { get; private set; }
    public string Action { get; private set; }      // "Created", "Posted", "Voided"
    public string ChangedBy { get; private set; }
    public DateTime ChangedDate { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }

    public static FinancialAuditEntry Create(
        Guid tenantId,
        string entityType,
        long entityId,
        string action,
        string changedBy,
        object? oldValues,
        object? newValues)
    {
        return new FinancialAuditEntry
        {
            TenantId = tenantId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            ChangedBy = changedBy,
            ChangedDate = DateTime.UtcNow,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null
        };
    }
}
```

✅ **Domain events for financial operations**:

```csharp
public class InvoicePostedEvent : DomainEvent
{
    public long InvoiceId { get; }
    public string InvoiceNumber { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }
    public DateTime PostedDate { get; }
    public string PostedBy { get; }
}

public class PaymentReceivedEvent : DomainEvent
{
    public long PaymentId { get; }
    public long InvoiceId { get; }
    public decimal Amount { get; }
    public string PaymentMethod { get; }
    public DateTime ReceivedDate { get; }
}
```

### 7. Immutability of Posted Transactions

✅ **Posted financial records cannot be modified**:

```csharp
public class Invoice : AggregateRoot
{
    public InvoiceStatus Status { get; private set; }

    public void UpdateLineItem(long lineItemId, ...)
    {
        if (Status == InvoiceStatus.Posted)
            throw new InvalidOperationException("Cannot modify posted invoices");

        // Update logic...
    }

    public void Void(string reason)
    {
        if (Status != InvoiceStatus.Posted)
            throw new InvalidOperationException("Can only void posted invoices");

        // Create reversing entry instead of deleting
        Status = InvoiceStatus.Voided;
        VoidedDate = DateTime.UtcNow;
        VoidReason = reason;

        AddDomainEvent(new InvoiceVoidedEvent(Id, TenantId, reason));
    }
}
```

✅ **Soft delete for financial records**:

```csharp
// NEVER physically delete financial records
public class Invoice : AggregateRoot
{
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedDate { get; private set; }
    public string? DeletionReason { get; private set; }

    public void Delete(string reason)
    {
        if (Status == InvoiceStatus.Posted)
            throw new InvalidOperationException("Cannot delete posted invoices. Use Void instead.");

        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
        DeletionReason = reason;
    }
}
```

## Common Financial Calculation Patterns

### Invoice Total Calculation

```csharp
public class Invoice : AggregateRoot
{
    private readonly List<InvoiceLineItem> _lineItems;

    public Money CalculateSubtotal()
    {
        if (!_lineItems.Any())
            return new Money(0, Currency);

        var total = _lineItems.Sum(x => x.CalculateTotal());
        return new Money(total, Currency);
    }

    public Money CalculateTax(decimal taxRate)
    {
        var subtotal = CalculateSubtotal();
        var taxAmount = subtotal.Amount * taxRate;
        return new Money(taxAmount, Currency);
    }

    public Money CalculateTotal(decimal taxRate)
    {
        var subtotal = CalculateSubtotal();
        var tax = CalculateTax(taxRate);
        return subtotal.Add(tax);
    }
}

public class InvoiceLineItem : Entity
{
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }

    public decimal CalculateTotal()
    {
        var lineTotal = Quantity * UnitPrice.Amount;
        var discount = lineTotal * (DiscountPercent / 100);
        var netAmount = lineTotal - discount;

        return Math.Round(netAmount, 2, MidpointRounding.AwayFromZero);
    }
}
```

### Payment Application

```csharp
public class Invoice : AggregateRoot
{
    public Money TotalAmount { get; private set; }
    public Money AmountPaid { get; private set; }

    public Money GetAmountDue()
    {
        var due = TotalAmount.Amount - AmountPaid.Amount;
        return new Money(due, TotalAmount.Currency);
    }

    public void ApplyPayment(Money payment)
    {
        if (payment.Currency != TotalAmount.Currency)
            throw new InvalidOperationException("Payment currency must match invoice currency");

        if (Status != InvoiceStatus.Posted)
            throw new InvalidOperationException("Can only apply payments to posted invoices");

        var newAmountPaid = AmountPaid.Add(payment);

        if (newAmountPaid.Amount > TotalAmount.Amount)
            throw new InvalidOperationException("Payment exceeds invoice total");

        AmountPaid = newAmountPaid;

        // Update status if fully paid
        if (AmountPaid.Amount == TotalAmount.Amount)
        {
            Status = InvoiceStatus.Paid;
            PaidDate = DateTime.UtcNow;
        }

        AddDomainEvent(new PaymentAppliedEvent(Id, payment.Amount));
    }
}
```

## Financial Accuracy Tests

### Test Template

```csharp
public class InvoiceFinancialTests
{
    [Theory]
    [InlineData(100.00, 10, 0, 1000.00)]      // Quantity * Price
    [InlineData(100.50, 10, 0, 1005.00)]      // With cents
    [InlineData(100.00, 10, 10, 900.00)]      // With 10% discount
    [InlineData(99.99, 3, 5, 284.97)]         // Complex calculation
    public void CalculateLineTotal_ShouldBeAccurate(
        decimal unitPrice,
        int quantity,
        decimal discountPercent,
        decimal expected)
    {
        // Arrange
        var lineItem = InvoiceLineItem.Create(
            new Money(unitPrice, "USD"),
            quantity,
            discountPercent
        );

        // Act
        var total = lineItem.CalculateTotal();

        // Assert
        total.Should().Be(expected);
    }

    [Fact]
    public void CalculateInvoiceTotal_WithTax_ShouldBeAccurate()
    {
        // Arrange
        var invoice = Invoice.Create(...);
        invoice.AddLineItem(new Money(100.00m, "USD"), 1, 0);
        invoice.AddLineItem(new Money(50.00m, "USD"), 2, 0);
        // Subtotal: $200.00

        // Act
        var total = invoice.CalculateTotal(taxRate: 0.08m); // 8% tax

        // Assert
        total.Amount.Should().Be(216.00m); // $200 + $16 tax
    }

    [Fact]
    public void ApplyPayment_Overpayment_ShouldThrowException()
    {
        // Arrange
        var invoice = Invoice.Create(...);
        invoice.AddLineItem(new Money(100.00m, "USD"), 1, 0);
        invoice.Post();

        // Act & Assert
        invoice.Invoking(i => i.ApplyPayment(new Money(150.00m, "USD")))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*exceeds invoice total*");
    }
}
```

## Validation Report Format

```
## Financial Accuracy Verification Report

### Summary
- Module: [Billing / Financial / etc.]
- Date: [YYYY-MM-DD]
- Verified By: Claude Code
- Status: ✅ PASS / ⚠️ WARNINGS / ❌ FAIL

### Decimal Precision
✅ All money fields use decimal type
✅ Database columns use DECIMAL(18,2)
✅ Value objects enforce precision

### Rounding
✅ Consistent MidpointRounding.AwayFromZero
✅ Rounding applied at appropriate points
⚠️ Warning: Check intermediate rounding in InvoiceService.cs:123

### Double-Entry Bookkeeping
✅ JournalEntry validates balanced entries
✅ Post() method enforces debit = credit
✅ Tests verify unbalanced entries rejected

### Currency Handling
✅ Money value object used throughout
✅ Currency mixing prevented
✅ ISO 4217 codes enforced

### Audit Trail
✅ Domain events for all financial operations
✅ FinancialAuditEntry records all changes
⚠️ Missing audit event for Payment.Void()

### Immutability
✅ Posted records cannot be modified
✅ Soft delete implemented
✅ Void functionality for reversals

### Recommendations
1. Add audit event for Payment.Void() operation
2. Review intermediate rounding in InvoiceService
3. Add integration test for trial balance verification
```

## Best Practices

- ✅ **ALWAYS** use `decimal` for money, NEVER `float` or `double`
- ✅ **ALWAYS** use `DECIMAL(18,2)` in database
- ✅ **ALWAYS** round with `MidpointRounding.AwayFromZero`
- ✅ **ALWAYS** validate debit = credit before posting
- ✅ **ALWAYS** use Money value object for currency support
- ✅ **ALWAYS** maintain complete audit trail
- ✅ **NEVER** modify posted financial records
- ✅ **NEVER** physically delete financial records (soft delete only)
- ✅ **NEVER** allow currency mixing in calculations
- ✅ **ALWAYS** test financial calculations with edge cases
