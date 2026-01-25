using ERP.Shared.Constants;

namespace ERP.Domain.Common.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency.
/// Immutable value object for handling money in the system.
/// </summary>
public class Money : ValueObject
{
    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency code (ISO 4217).
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Private parameterless constructor for EF Core.
    /// </summary>
    private Money()
    {
        Currency = string.Empty;
    }

    /// <summary>
    /// Constructor for EF Core materialization (matches persisted properties).
    /// </summary>
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency code (e.g., "USD", "EUR").</param>
    /// <param name="allowNegative">Whether to allow negative amounts. Default is true for flexibility in calculations.</param>
    public Money(decimal amount, string currency, bool allowNegative = true)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        if (currency.Length != BusinessConstants.Currency.CurrencyCodeLength)
            throw new ArgumentException($"Currency must be a {BusinessConstants.Currency.CurrencyCodeLength}-letter ISO 4217 code.", nameof(currency));

        if (!allowNegative && amount < 0)
            throw new ArgumentException($"Amount cannot be negative. Received: {amount}", nameof(amount));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Creates a Money instance with zero amount.
    /// </summary>
    public static Money Zero(string currency) => new Money(0, currency);

    /// <summary>
    /// Creates a Money instance that must be positive (> 0).
    /// Use for amounts that should never be zero or negative (e.g., unit prices, rates).
    /// </summary>
    public static Money CreatePositive(decimal amount, string currency)
    {
        if (amount <= 0)
            throw new ArgumentException($"Amount must be positive (greater than zero). Received: {amount}", nameof(amount));

        return new Money(amount, currency, allowNegative: false);
    }

    /// <summary>
    /// Creates a Money instance that must be non-negative (>= 0).
    /// Use for amounts that can be zero but not negative (e.g., invoice totals, payment amounts, budgets).
    /// </summary>
    public static Money CreateNonNegative(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException($"Amount cannot be negative. Received: {amount}", nameof(amount));

        return new Money(amount, currency, allowNegative: false);
    }

    /// <summary>
    /// Adds two money amounts.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts a money amount from this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies the money amount by a factor.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Divides the money amount by a divisor.
    /// </summary>
    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide money by zero.");

        return new Money(Amount / divisor, Currency);
    }

    /// <summary>
    /// Determines if this money amount is greater than another.
    /// </summary>
    public bool IsGreaterThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {Currency} and {other.Currency}");

        return Amount > other.Amount;
    }

    /// <summary>
    /// Determines if this money amount is less than another.
    /// </summary>
    public bool IsLessThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {Currency} and {other.Currency}");

        return Amount < other.Amount;
    }

    /// <summary>
    /// Determines if this money amount is zero.
    /// </summary>
    public bool IsZero() => Amount == 0;

    /// <summary>
    /// Determines if this money amount is positive.
    /// </summary>
    public bool IsPositive() => Amount > 0;

    /// <summary>
    /// Determines if this money amount is negative.
    /// </summary>
    public bool IsNegative() => Amount < 0;

    /// <summary>
    /// Returns the absolute value of the money amount.
    /// </summary>
    public Money Abs()
    {
        return new Money(Math.Abs(Amount), Currency);
    }

    /// <summary>
    /// Returns a rounded money amount.
    /// </summary>
    public Money Round(int decimals = 2)
    {
        return new Money(Math.Round(Amount, decimals), Currency);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
    {
        return $"{Amount:N2} {Currency}";
    }

    /// <summary>
    /// Addition operator.
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        return left.Add(right);
    }

    /// <summary>
    /// Subtraction operator.
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        return left.Subtract(right);
    }

    /// <summary>
    /// Multiplication operator.
    /// </summary>
    public static Money operator *(Money money, decimal factor)
    {
        return money.Multiply(factor);
    }

    /// <summary>
    /// Division operator.
    /// </summary>
    public static Money operator /(Money money, decimal divisor)
    {
        return money.Divide(divisor);
    }
}
