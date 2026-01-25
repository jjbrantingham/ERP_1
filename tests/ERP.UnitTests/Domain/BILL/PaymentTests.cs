using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.Common.ValueObjects;

namespace ERP.UnitTests.Domain.BILL;

/// <summary>
/// Unit tests for Payment entity
/// </summary>
public class PaymentTests
{
    [Fact]
    public void Create_WithValidData_CreatesPayment()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var paymentNumber = "PAY-2024-001";
        var clientId = 1L;
        var amount = new Money(1000.00m, "USD");
        var paymentDate = DateTime.UtcNow;
        var paymentMethod = PaymentMethod.Check;

        // Act
        var payment = Payment.Create(
            tenantId,
            paymentNumber,
            clientId,
            amount,
            paymentMethod,
            paymentDate);

        // Assert
        Assert.NotNull(payment);
        Assert.Equal(tenantId, payment.TenantId);
        Assert.Equal(paymentNumber, payment.PaymentNumber);
        Assert.Equal(clientId, payment.ClientId);
        Assert.Equal(amount.Amount, payment.Amount.Amount);
        Assert.Equal(paymentDate.Date, payment.PaymentDate.Date);
        Assert.Equal(paymentMethod, payment.Method);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void Create_WithInvoiceId_LinksToInvoice()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceId = 42L;
        var amount = new Money(500.00m, "USD");

        // Act
        var payment = Payment.Create(
            tenantId,
            "PAY-2024-002",
            1L,
            amount,
            PaymentMethod.ACH,
            DateTime.UtcNow,
            invoiceId: invoiceId);

        // Assert
        Assert.Equal(invoiceId, payment.InvoiceId);
    }

    [Fact]
    public void Create_WithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var amount = new Money(0m, "USD");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Payment.Create(
                Guid.NewGuid(),
                "PAY-2024-003",
                1L,
                amount,
                PaymentMethod.Cash,
                DateTime.UtcNow));
        Assert.Contains("greater than zero", exception.Message);
    }

    [Theory]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Check)]
    [InlineData(PaymentMethod.CreditCard)]
    [InlineData(PaymentMethod.Wire)]
    [InlineData(PaymentMethod.ACH)]
    public void Create_WithVariousPaymentMethods_CreatesSuccessfully(PaymentMethod method)
    {
        // Act
        var payment = Payment.Create(
            Guid.NewGuid(),
            "PAY-2024-004",
            1L,
            new Money(500.00m, "USD"),
            method,
            DateTime.UtcNow);

        // Assert
        Assert.Equal(method, payment.Method);
    }

    [Fact]
    public void Clear_WhenPending_SetsStatusToCleared()
    {
        // Arrange
        var payment = CreateTestPayment();

        // Act
        payment.Clear();

        // Assert
        Assert.Equal(PaymentStatus.Cleared, payment.Status);
        Assert.NotNull(payment.ClearedDate);
    }

    [Fact]
    public void Clear_WhenAlreadyCleared_ThrowsInvalidOperationException()
    {
        // Arrange
        var payment = CreateTestPayment();
        payment.Clear();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => payment.Clear());
    }

    [Fact]
    public void MarkFailed_WhenPending_SetsStatusToFailed()
    {
        // Arrange
        var payment = CreateTestPayment();
        var reason = "Insufficient funds";

        // Act
        payment.MarkFailed(reason);

        // Assert
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Contains(reason, payment.Notes!);
    }

    private Payment CreateTestPayment()
    {
        return Payment.Create(
            Guid.NewGuid(),
            "PAY-TEST-001",
            1L,
            new Money(1000.00m, "USD"),
            PaymentMethod.Check,
            DateTime.UtcNow);
    }
}
