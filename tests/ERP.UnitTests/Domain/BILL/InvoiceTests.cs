using ERP.Domain.BILL.Entities;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.ValueObjects;
using ERP.Domain.Common.ValueObjects;

namespace ERP.UnitTests.Domain.BILL;

/// <summary>
/// Unit tests for Invoice entity
/// CRITICAL: Tests billing calculation accuracy
/// </summary>
public class InvoiceTests
{
    [Fact]
    public void CalculateSubtotal_WithNoLineItems_ReturnsZero()
    {
        // Arrange
        var invoice = CreateTestInvoice();

        // Act
        var subtotal = invoice.CalculateSubtotal();

        // Assert
        Assert.Equal(0m, subtotal.Amount);
    }

    [Fact]
    public void CalculateSubtotal_WithLineItems_ReturnsCorrectSum()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddLineItem("Item 1", 2m, new Money(100m, "USD"), 0m); // $200
        invoice.AddLineItem("Item 2", 3m, new Money(50m, "USD"), 0m);  // $150

        // Act
        var subtotal = invoice.CalculateSubtotal();

        // Assert
        Assert.Equal(350m, subtotal.Amount);
    }

    [Fact]
    public void CalculateSubtotal_WithDiscounts_AppliesDiscountCorrectly()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddLineItem("Item 1", 1m, new Money(100m, "USD"), 10m); // $100 - 10% = $90

        // Act
        var subtotal = invoice.CalculateSubtotal();

        // Assert
        Assert.Equal(90m, subtotal.Amount);
    }

    [Fact]
    public void CalculateTax_AppliesTaxRateToSubtotal()
    {
        // Arrange
        var invoice = CreateTestInvoice(taxRate: 0.08m); // 8% tax
        invoice.AddLineItem("Item 1", 1m, new Money(100m, "USD"), 0m); // $100

        // Act
        var tax = invoice.CalculateTax();

        // Assert
        Assert.Equal(8m, tax.Amount); // 8% of $100
    }

    [Fact]
    public void CalculateTotal_IncludesSubtotalPlusTax()
    {
        // Arrange
        var invoice = CreateTestInvoice(taxRate: 0.10m); // 10% tax
        invoice.AddLineItem("Item 1", 2m, new Money(100m, "USD"), 0m); // $200

        // Act
        var total = invoice.CalculateTotal();

        // Assert
        Assert.Equal(220m, total.Amount); // $200 + $20 tax
    }

    [Fact]
    public void CalculateAmountDue_ReturnsCorrectAmountAfterPayment()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddLineItem("Item 1", 1m, new Money(100m, "USD"), 0m);
        invoice.Post();
        invoice.ApplyPayment(new Money(30m, "USD"));

        // Act
        var amountDue = invoice.CalculateAmountDue();

        // Assert
        Assert.Equal(70m, amountDue.Amount); // $100 - $30
    }

    [Fact]
    public void ApplyPayment_WhenFullPayment_SetsStatusToPaid()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddLineItem("Item 1", 1m, new Money(100m, "USD"), 0m);
        invoice.Post();

        // Act
        invoice.ApplyPayment(new Money(100m, "USD"));

        // Assert
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.NotNull(invoice.PaidDate);
    }

    [Fact]
    public void ApplyPayment_WhenPartialPayment_SetsStatusToPartiallyPaid()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddLineItem("Item 1", 1m, new Money(100m, "USD"), 0m);
        invoice.Post();

        // Act
        invoice.ApplyPayment(new Money(50m, "USD"));

        // Assert
        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
    }

    [Fact]
    public void ApplyPayment_WhenExceedsTotal_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = CreateTestInvoice();
        invoice.AddLineItem("Item 1", 1m, new Money(100m, "USD"), 0m);
        invoice.Post();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => invoice.ApplyPayment(new Money(150m, "USD")));
        Assert.Contains("exceeds amount due", exception.Message);
    }

    [Fact]
    public void ApplyPayment_WithWrongCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = CreateTestInvoice(currency: "USD");
        invoice.AddLineItem("Item 1", 1m, new Money(100m, "USD"), 0m);
        invoice.Post();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => invoice.ApplyPayment(new Money(50m, "EUR")));
        Assert.Contains("currency", exception.Message);
    }

    [Fact]
    public void AddLineItem_WithWrongCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var invoice = CreateTestInvoice(currency: "USD");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => invoice.AddLineItem("Item 1", 1m, new Money(100m, "EUR"), 0m));
        Assert.Contains("currency", exception.Message);
    }

    // Helper method to create test invoices
    private Invoice CreateTestInvoice(decimal taxRate = 0m, string currency = "USD")
    {
        var tenantId = Guid.NewGuid();
        var invoiceNumber = new InvoiceNumber("INV-2024-001");
        return Invoice.Create(
            tenantId,
            invoiceNumber,
            clientId: 1,
            invoiceDate: DateTime.UtcNow,
            dueDate: DateTime.UtcNow.AddDays(30),
            billingMode: BillingMode.TimeAndMaterials,
            currency: currency,
            taxRate: taxRate);
    }
}
