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
        var invoiceId = 1L;
        var amount = new Money(1000.00m, "USD");
        var paymentDate = DateTime.UtcNow;
        var paymentMethod = PaymentMethod.Check;

        // Act
        var payment = Payment.Create(
            tenantId,
            invoiceId,
            amount,
            paymentDate,
            paymentMethod);

        // Assert
        Assert.NotNull(payment);
        Assert.Equal(tenantId, payment.TenantId);
        Assert.Equal(invoiceId, payment.InvoiceId);
        Assert.Equal(amount, payment.Amount);
        Assert.Equal(paymentDate.Date, payment.PaymentDate);
        Assert.Equal(paymentMethod, payment.PaymentMethod);
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var amount = new Money(-100.00m, "USD");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Payment.Create(
                Guid.NewGuid(),
                1L,
                amount,
                DateTime.UtcNow,
                PaymentMethod.Cash));
        Assert.Contains("Payment amount must be positive", exception.Message);
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
                1L,
                amount,
                DateTime.UtcNow,
                PaymentMethod.Cash));
        Assert.Contains("Payment amount must be positive", exception.Message);
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
            1L,
            new Money(500.00m, "USD"),
            DateTime.UtcNow,
            method);

        // Assert
        Assert.Equal(method, payment.PaymentMethod);
    }
}
