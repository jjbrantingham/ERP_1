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
        var reportNumber = "EXP-2024-001";
        var reportDate = DateTime.UtcNow;

        // Act
        var report = ExpenseReport.Create(tenantId, employeeId, reportNumber, reportDate);

        // Assert
        Assert.NotNull(report);
        Assert.Equal(tenantId, report.TenantId);
        Assert.Equal(employeeId, report.EmployeeId);
        Assert.Equal(reportNumber, report.ReportNumber);
        Assert.Equal(reportDate.Date, report.ReportDate);
        Assert.Equal(ExpenseStatus.Draft, report.Status);
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
            "Flight to client site",
            new Money(150.00m, "USD"));

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
        Assert.Equal(ExpenseStatus.Submitted, report.Status);
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
        Assert.Contains("empty", exception.Message.ToLower());
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
        Assert.Equal(ExpenseStatus.Approved, report.Status);
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
        Assert.Equal(ExpenseStatus.Rejected, report.Status);
    }

    private ExpenseReport CreateTestExpenseReport()
    {
        return ExpenseReport.Create(
            Guid.NewGuid(),
            employeeId: 1L,
            reportNumber: "EXP-TEST-001",
            reportDate: DateTime.UtcNow,
            purpose: "Test expense report");
    }

    private ExpenseReport CreateTestExpenseReportWithItem()
    {
        var report = CreateTestExpenseReport();
        var item = ExpenseItem.Create(
            report.TenantId,
            report.Id,
            DateTime.UtcNow,
            ExpenseCategory.Meals,
            "Client lunch",
            new Money(50.00m, "USD"));
        report.AddItem(item);
        return report;
    }
}
