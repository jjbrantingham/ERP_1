using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Enums;

namespace ERP.UnitTests.Domain.TE;

/// <summary>
/// Unit tests for Timesheet entity
/// CRITICAL: Tests time tracking approval workflow
/// </summary>
public class TimesheetTests
{
    [Fact]
    public void Create_WithValidData_CreatesTimesheet()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var employeeId = 1L;
        var periodStart = new DateTime(2024, 1, 1);
        var periodEnd = new DateTime(2024, 1, 7);

        // Act
        var timesheet = Timesheet.Create(tenantId, employeeId, periodStart, periodEnd);

        // Assert
        Assert.NotNull(timesheet);
        Assert.Equal(tenantId, timesheet.TenantId);
        Assert.Equal(employeeId, timesheet.EmployeeId);
        Assert.Equal(periodStart.Date, timesheet.PeriodStart);
        Assert.Equal(periodEnd.Date, timesheet.PeriodEnd);
        Assert.Equal(TimesheetStatus.Draft, timesheet.Status);
        Assert.Equal(0, timesheet.TotalHours);
        Assert.Empty(timesheet.Entries);
    }

    [Fact]
    public void Create_WhenPeriodStartAfterOrEqualPeriodEnd_ThrowsArgumentException()
    {
        // Arrange
        var periodStart = new DateTime(2024, 1, 7);
        var periodEnd = new DateTime(2024, 1, 1);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Timesheet.Create(Guid.NewGuid(), 1L, periodStart, periodEnd));
        Assert.Contains("Period start must be before period end", exception.Message);
    }

    [Fact]
    public void AddEntry_WhenDraft_AddsEntryAndRecalculatesTotalHours()
    {
        // Arrange
        var timesheet = CreateTestTimesheet();
        var entry = TimesheetEntry.Create(
            timesheet.TenantId,
            timesheet.Id,
            DateTime.UtcNow,
            8.0m,
            projectId: 1L);

        // Act
        timesheet.AddEntry(entry);

        // Assert
        Assert.Single(timesheet.Entries);
        Assert.Equal(8.0m, timesheet.TotalHours);
        Assert.NotNull(timesheet.ModifiedDate);
    }

    [Fact]
    public void AddEntry_WithMultipleEntries_CalculatesTotalCorrectly()
    {
        // Arrange
        var timesheet = CreateTestTimesheet();
        var entry1 = TimesheetEntry.Create(timesheet.TenantId, timesheet.Id, DateTime.UtcNow, 8.0m);
        var entry2 = TimesheetEntry.Create(timesheet.TenantId, timesheet.Id, DateTime.UtcNow.AddDays(1), 6.5m);
        var entry3 = TimesheetEntry.Create(timesheet.TenantId, timesheet.Id, DateTime.UtcNow.AddDays(2), 7.5m);

        // Act
        timesheet.AddEntry(entry1);
        timesheet.AddEntry(entry2);
        timesheet.AddEntry(entry3);

        // Assert
        Assert.Equal(3, timesheet.Entries.Count);
        Assert.Equal(22.0m, timesheet.TotalHours); // 8 + 6.5 + 7.5
    }

    [Fact]
    public void AddEntry_WhenNotDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();
        timesheet.Submit();
        var entry = TimesheetEntry.Create(timesheet.TenantId, timesheet.Id, DateTime.UtcNow, 8.0m);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => timesheet.AddEntry(entry));
        Assert.Contains("Cannot add entries to a non-draft timesheet", exception.Message);
    }

    [Fact]
    public void RemoveEntry_WhenDraft_RemovesEntryAndRecalculatesTotalHours()
    {
        // Arrange
        var timesheet = CreateTestTimesheet();
        var entry1 = TimesheetEntry.Create(timesheet.TenantId, timesheet.Id, DateTime.UtcNow, 8.0m);
        var entry2 = TimesheetEntry.Create(timesheet.TenantId, timesheet.Id, DateTime.UtcNow.AddDays(1), 6.0m);
        timesheet.AddEntry(entry1);
        timesheet.AddEntry(entry2);

        // Act
        timesheet.RemoveEntry(entry1.Id);

        // Assert
        Assert.Single(timesheet.Entries);
        Assert.Equal(6.0m, timesheet.TotalHours);
    }

    [Fact]
    public void RemoveEntry_WhenNotDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();
        var entryId = timesheet.Entries.First().Id;
        timesheet.Submit();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => timesheet.RemoveEntry(entryId));
        Assert.Contains("Cannot remove entries from a non-draft timesheet", exception.Message);
    }

    [Fact]
    public void Submit_WhenDraftWithEntries_SubmitsSuccessfully()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();

        // Act
        timesheet.Submit();

        // Assert
        Assert.Equal(TimesheetStatus.Submitted, timesheet.Status);
        Assert.NotNull(timesheet.SubmittedDate);
        Assert.Single(timesheet.DomainEvents);
        Assert.IsType<TimesheetSubmittedEvent>(timesheet.DomainEvents.First());
    }

    [Fact]
    public void Submit_WhenRecalled_CanResubmit()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();
        timesheet.Submit();
        timesheet.Recall();

        // Act
        timesheet.Submit();

        // Assert
        Assert.Equal(TimesheetStatus.Submitted, timesheet.Status);
    }

    [Fact]
    public void Submit_WhenEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var timesheet = CreateTestTimesheet();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => timesheet.Submit());
        Assert.Contains("Cannot submit empty timesheet", exception.Message);
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ThrowsInvalidOperationException()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();
        timesheet.Submit();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => timesheet.Submit());
        Assert.Contains("Can only submit draft or recalled timesheets", exception.Message);
    }

    [Fact]
    public void Approve_WhenSubmitted_ApprovesSuccessfully()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();
        timesheet.Submit();
        var approverUserId = 10L;
        var comments = "Approved - looks good";

        // Act
        timesheet.Approve(approverUserId, comments);

        // Assert
        Assert.Equal(TimesheetStatus.Approved, timesheet.Status);
        Assert.NotNull(timesheet.ApprovedDate);
        Assert.Equal(approverUserId, timesheet.ApprovedByUserId);
        Assert.Equal(comments, timesheet.ApprovalComments);
        Assert.Contains(timesheet.DomainEvents, e => e is TimesheetApprovedEvent);
    }

    [Fact]
    public void Approve_WhenNotSubmitted_ThrowsInvalidOperationException()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => timesheet.Approve(10L));
        Assert.Contains("Can only approve submitted timesheets", exception.Message);
    }

    [Fact]
    public void Reject_WhenSubmittedWithReason_RejectsSuccessfully()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();
        timesheet.Submit();
        var rejectorUserId = 10L;
        var reason = "Hours exceed project budget";

        // Act
        timesheet.Reject(rejectorUserId, reason);

        // Assert
        Assert.Equal(TimesheetStatus.Rejected, timesheet.Status);
        Assert.Equal(rejectorUserId, timesheet.ApprovedByUserId);
        Assert.Equal(reason, timesheet.ApprovalComments);
        Assert.Contains(timesheet.DomainEvents, e => e is TimesheetRejectedEvent);
    }

    [Fact]
    public void Reject_WithoutReason_ThrowsArgumentException()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();
        timesheet.Submit();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => timesheet.Reject(10L, "   "));
        Assert.Contains("Rejection reason is required", exception.Message);
    }

    [Fact]
    public void Reject_WhenNotSubmitted_ThrowsInvalidOperationException()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => timesheet.Reject(10L, "Some reason"));
        Assert.Contains("Can only reject submitted timesheets", exception.Message);
    }

    [Fact]
    public void Recall_WhenSubmitted_RecallsSuccessfully()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();
        timesheet.Submit();

        // Act
        timesheet.Recall();

        // Assert
        Assert.Equal(TimesheetStatus.Recalled, timesheet.Status);
        Assert.NotNull(timesheet.ModifiedDate);
    }

    [Fact]
    public void Recall_WhenNotSubmitted_ThrowsInvalidOperationException()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => timesheet.Recall());
        Assert.Contains("Can only recall submitted timesheets", exception.Message);
    }

    [Fact]
    public void CompleteWorkflow_DraftToSubmittedToApproved_WorksCorrectly()
    {
        // Arrange
        var timesheet = CreateTestTimesheet();
        var entry = TimesheetEntry.Create(timesheet.TenantId, timesheet.Id, DateTime.UtcNow, 8.0m);

        // Act & Assert - Draft
        Assert.Equal(TimesheetStatus.Draft, timesheet.Status);

        timesheet.AddEntry(entry);
        Assert.Single(timesheet.Entries);

        // Submit
        timesheet.Submit();
        Assert.Equal(TimesheetStatus.Submitted, timesheet.Status);
        Assert.NotNull(timesheet.SubmittedDate);

        // Approve
        timesheet.Approve(10L, "Approved");
        Assert.Equal(TimesheetStatus.Approved, timesheet.Status);
        Assert.NotNull(timesheet.ApprovedDate);
    }

    [Fact]
    public void CompleteWorkflow_DraftToSubmittedToRejected_WorksCorrectly()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();

        // Act - Submit
        timesheet.Submit();
        Assert.Equal(TimesheetStatus.Submitted, timesheet.Status);

        // Reject
        timesheet.Reject(10L, "Invalid hours");
        Assert.Equal(TimesheetStatus.Rejected, timesheet.Status);
    }

    [Fact]
    public void CompleteWorkflow_SubmitRecallResubmit_WorksCorrectly()
    {
        // Arrange
        var timesheet = CreateTestTimesheetWithEntry();

        // Act - Submit
        timesheet.Submit();
        Assert.Equal(TimesheetStatus.Submitted, timesheet.Status);

        // Recall
        timesheet.Recall();
        Assert.Equal(TimesheetStatus.Recalled, timesheet.Status);

        // Can modify after recall
        var entry = TimesheetEntry.Create(timesheet.TenantId, timesheet.Id, DateTime.UtcNow, 4.0m);
        timesheet.AddEntry(entry);

        // Resubmit
        timesheet.Submit();
        Assert.Equal(TimesheetStatus.Submitted, timesheet.Status);
    }

    // Helper methods
    private Timesheet CreateTestTimesheet()
    {
        return Timesheet.Create(
            Guid.NewGuid(),
            employeeId: 1L,
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 7),
            "Test timesheet");
    }

    private Timesheet CreateTestTimesheetWithEntry()
    {
        var timesheet = CreateTestTimesheet();
        var entry = TimesheetEntry.Create(
            timesheet.TenantId,
            timesheet.Id,
            DateTime.UtcNow,
            8.0m,
            projectId: 1L);
        timesheet.AddEntry(entry);
        return timesheet;
    }
}
