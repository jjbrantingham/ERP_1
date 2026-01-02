using ERP.Domain.FIN.Entities;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.ValueObjects;

namespace ERP.UnitTests.Domain.FIN;

/// <summary>
/// Unit tests for JournalEntry entity
/// CRITICAL: Tests double-entry bookkeeping validation
/// </summary>
public class JournalEntryTests
{
    [Fact]
    public void IsBalanced_WhenDebitsEqualCredits_ReturnsTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entryNumber = JournalEntryNumber.Create("JE-2024-001");
        var journalEntry = JournalEntry.Create(
            tenantId,
            entryNumber,
            DateTime.UtcNow,
            JournalEntryType.General,
            "Test Entry",
            "2024-01");

        // Add balanced lines: $1000 debit, $1000 credit
        journalEntry.AddLine(1, 1000m, 0m, "Debit line");
        journalEntry.AddLine(2, 0m, 1000m, "Credit line");

        // Act
        var isBalanced = journalEntry.IsBalanced();

        // Assert
        Assert.True(isBalanced);
    }

    [Fact]
    public void IsBalanced_WhenDebitsDoNotEqualCredits_ReturnsFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entryNumber = JournalEntryNumber.Create("JE-2024-002");
        var journalEntry = JournalEntry.Create(
            tenantId,
            entryNumber,
            DateTime.UtcNow,
            JournalEntryType.General,
            "Unbalanced Entry",
            "2024-01");

        // Add unbalanced lines: $1000 debit, $500 credit
        journalEntry.AddLine(1, 1000m, 0m, "Debit line");
        journalEntry.AddLine(2, 0m, 500m, "Credit line");

        // Act
        var isBalanced = journalEntry.IsBalanced();

        // Assert
        Assert.False(isBalanced);
    }

    [Fact]
    public void Post_WhenBalanced_SetsStatusToPosted()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entryNumber = JournalEntryNumber.Create("JE-2024-003");
        var journalEntry = JournalEntry.Create(
            tenantId,
            entryNumber,
            DateTime.UtcNow,
            JournalEntryType.General,
            "Test Entry",
            "2024-01");

        journalEntry.AddLine(1, 1000m, 0m, "Debit");
        journalEntry.AddLine(2, 0m, 1000m, "Credit");

        // Act
        journalEntry.Post("TestUser");

        // Assert
        Assert.Equal(JournalEntryStatus.Posted, journalEntry.Status);
        Assert.NotNull(journalEntry.PostedDate);
        Assert.Equal("TestUser", journalEntry.PostedBy);
    }

    [Fact]
    public void Post_WhenNotBalanced_ThrowsInvalidOperationException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entryNumber = JournalEntryNumber.Create("JE-2024-004");
        var journalEntry = JournalEntry.Create(
            tenantId,
            entryNumber,
            DateTime.UtcNow,
            JournalEntryType.General,
            "Unbalanced Entry",
            "2024-01");

        journalEntry.AddLine(1, 1000m, 0m, "Debit");
        journalEntry.AddLine(2, 0m, 500m, "Credit");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => journalEntry.Post("TestUser"));
        Assert.Contains("not balanced", exception.Message);
    }

    [Fact]
    public void CalculateDebitTotal_ReturnsCorrectSum()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entryNumber = JournalEntryNumber.Create("JE-2024-005");
        var journalEntry = JournalEntry.Create(
            tenantId,
            entryNumber,
            DateTime.UtcNow,
            JournalEntryType.General,
            "Test Entry",
            "2024-01");

        journalEntry.AddLine(1, 500m, 0m, "Debit 1");
        journalEntry.AddLine(2, 750m, 0m, "Debit 2");
        journalEntry.AddLine(3, 0m, 1250m, "Credit");

        // Act
        var debitTotal = journalEntry.CalculateDebitTotal();

        // Assert
        Assert.Equal(1250m, debitTotal);
    }

    [Fact]
    public void CalculateCreditTotal_ReturnsCorrectSum()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var entryNumber = JournalEntryNumber.Create("JE-2024-006");
        var journalEntry = JournalEntry.Create(
            tenantId,
            entryNumber,
            DateTime.UtcNow,
            JournalEntryType.General,
            "Test Entry",
            "2024-01");

        journalEntry.AddLine(1, 1250m, 0m, "Debit");
        journalEntry.AddLine(2, 0m, 500m, "Credit 1");
        journalEntry.AddLine(3, 0m, 750m, "Credit 2");

        // Act
        var creditTotal = journalEntry.CalculateCreditTotal();

        // Assert
        Assert.Equal(1250m, creditTotal);
    }

    [Fact]
    public void IsBalanced_WithRoundingDifferences_HandlesCorrectly()
    {
        // Arrange - Test that rounding is handled correctly
        var tenantId = Guid.NewGuid();
        var entryNumber = JournalEntryNumber.Create("JE-2024-007");
        var journalEntry = JournalEntry.Create(
            tenantId,
            entryNumber,
            DateTime.UtcNow,
            JournalEntryType.General,
            "Rounding Test",
            "2024-01");

        // Add lines that should balance after rounding to 2 decimals
        journalEntry.AddLine(1, 100.005m, 0m, "Debit");  // Rounds to 100.01
        journalEntry.AddLine(2, 0m, 100.005m, "Credit"); // Rounds to 100.01

        // Act
        var isBalanced = journalEntry.IsBalanced();

        // Assert - Should be balanced after rounding
        Assert.True(isBalanced);
    }
}
