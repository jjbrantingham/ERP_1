# Generate Functional Tests Skill

## Purpose
Generate end-to-end functional tests for complete user workflows using Playwright or Selenium.

## Example Test

```csharp
[Fact]
public async Task CreateAndApproveTimesheet_CompleteWorkflow_Success()
{
    // Navigate to timesheets
    await Page.GotoAsync("/timesheets");
    
    // Create timesheet
    await Page.ClickAsync("button:has-text('Create Timesheet')");
    await Page.FillAsync("#weekStartDate", "2026-01-01");
    await Page.FillAsync("#hours-monday", "8");
    await Page.ClickAsync("button:has-text('Save')");
    
    // Submit for approval
    await Page.ClickAsync("button:has-text('Submit')");
    
    // Verify status
    var status = await Page.TextContentAsync(".status-badge");
    status.Should().Contain("Submitted");
    
    // Manager approves
    await LoginAsManager();
    await Page.GotoAsync("/timesheets/pending");
    await Page.ClickAsync("button:has-text('Approve')");
    
    // Verify approved
    var approvedStatus = await Page.TextContentAsync(".status-badge");
    approvedStatus.Should().Contain("Approved");
}
```

## Related Skills
- generate-integration-tests
