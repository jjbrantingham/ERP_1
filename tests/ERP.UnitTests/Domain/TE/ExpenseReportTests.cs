using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;
using ERP.Domain.Common.ValueObjects;

namespace ERP.UnitTests.Domain.TE;

/// <summary>
/// Unit tests for ExpenseReport entity
/// CRITICAL: Tests expense approval workflow
/// </summary>
public class ExpenseReportTests
{
    [Fact]
    public void Create_WithValidData_CreatesExpenseReport()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var employeeId = 1L;
        var reportDate = DateTime.UtcNow;

        // Act
        var report = ExpenseReport.Create(tenantId, employeeId, reportDate);

        // Assert
        Assert.NotNull(report);
        Assert.Equal(tenantId, report.TenantId);
        Assert.Equal(employeeId, report.EmployeeId);
        Assert.Equal(reportDate.Date, report.ReportDate);
        Assert.Equal(ExpenseReportStatus.Draft, report.Status);
        Assert.Empty(report.Items);
    }

    [Fact]
    public void AddItem_WhenDraft_AddsItemAndRecalculatesTotal()
    {
        // Arrange
        var report = CreateTestExpenseReport();
        var item = ExpenseItem.Create(
            report.TenantId,
            report.Id,
            DateTime.UtcNow,
            ExpenseCategory.Travel,
            new Money(150.00m, "USD"),
            "Flight to client site");

        // Act
        report.AddItem(item);

        // Assert
        Assert.Single(report.Items);
        Assert.Equal(150.00m, report.TotalAmount.Amount);
    }

    [Fact]
    public void Submit_WhenDraftWithItems_SubmitsSuccessfully()
    {
        // Arrange
        var report = CreateTestExpenseReportWithItem();

        // Act
        report.Submit();

        // Assert
        Assert.Equal(ExpenseReportStatus.Submitted, report.Status);
        Assert.NotNull(report.SubmittedDate);
    }

    [Fact]
    public void Submit_WhenEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var report = CreateTestExpenseReport();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => report.Submit());
        Assert.Contains("Cannot submit empty expense report", exception.Message);
    }

    [Fact]
    public void Approve_WhenSubmitted_ApprovesSuccessfully()
    {
        // Arrange
        var report = CreateTestExpenseReportWithItem();
        report.Submit();
        var approverUserId = 10L;

        // Act
        report.Approve(approverUserId, "Approved");

        // Assert
        Assert.Equal(ExpenseReportStatus.Approved, report.Status);
        Assert.NotNull(report.ApprovedDate);
        Assert.Equal(approverUserId, report.ApprovedByUserId);
    }

    [Fact]
    public void Reject_WithReason_RejectsSuccessfully()
    {
        // Arrange
        var report = CreateTestExpenseReportWithItem();
        report.Submit();

        // Act
        report.Reject(10L, "Missing receipts");

        // Assert
        Assert.Equal(ExpenseReportStatus.Rejected, report.Status);
    }

    private ExpenseReport CreateTestExpenseReport()
    {
        return ExpenseReport.Create(
            Guid.NewGuid(),
            employeeId: 1L,
            DateTime.UtcNow,
            "Test expense report");
    }

    private ExpenseReport CreateTestExpenseReportWithItem()
    {
        var report = CreateTestExpenseReport();
        var item = ExpenseItem.Create(
            report.TenantId,
            report.Id,
            DateTime.UtcNow,
            ExpenseCategory.Meals,
            new Money(50.00m, "USD"),
            "Client lunch");
        report.AddItem(item);
        return report;
    }
}
