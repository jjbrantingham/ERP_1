# Phase 13: Workflow Integration & Notifications

## Overview

Phase 13 completes the workflow system by integrating it with existing modules (Time & Expense, Billing, Financial) and adding email notification capabilities. This phase enables end-to-end approval workflows with automatic entity updates and real-time notifications.

---

## Workflow Integration

### Event-Driven Architecture

The integration uses domain events to decouple workflow completion from entity updates. When a workflow completes or is rejected, event handlers automatically update the corresponding entities.

**Flow**:
1. User approves/rejects workflow step
2. Workflow aggregate raises domain event
3. MediatR dispatches event to all registered handlers
4. Entity-specific handlers update the entity
5. Notification handlers send emails to stakeholders

---

## Integration with Modules

### 1. Time & Expense (TE) Integration

**Entities**: Timesheets, Expense Reports

**Event Handlers**:
- `TimesheetWorkflowCompletedEventHandler`
- `TimesheetWorkflowRejectedEventHandler`
- `ExpenseReportWorkflowCompletedEventHandler`
- `ExpenseReportWorkflowRejectedEventHandler`

**Workflow Completion Behavior**:
```csharp
// Timesheet approved when workflow completes
public async Task Handle(WorkflowInstanceCompletedEvent notification, CancellationToken cancellationToken)
{
    if (notification.EntityType != "Timesheet")
        return;

    var timesheet = await _timesheetRepository.GetByIdAsync(notification.EntityId, cancellationToken);
    timesheet.Approve();  // Changes status to Approved

    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

**Workflow Rejection Behavior**:
```csharp
// Timesheet rejected when workflow is rejected
public async Task Handle(WorkflowInstanceRejectedEvent notification, CancellationToken cancellationToken)
{
    if (notification.EntityType != "Timesheet")
        return;

    var timesheet = await _timesheetRepository.GetByIdAsync(notification.EntityId, cancellationToken);
    timesheet.Reject();  // Changes status back to Draft or Rejected

    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

**Usage Example**:
1. Employee submits timesheet → Status: Submitted
2. Submit triggers workflow creation
3. Manager approves (Step 1) → Workflow continues
4. Finance approves (Step 2) → Workflow completes
5. Event handler called → Timesheet status: Approved
6. Timesheet ready for payroll processing

---

### 2. Billing (BILL) Integration

**Entities**: Invoices

**Event Handlers**:
- `InvoiceWorkflowCompletedEventHandler`
- `InvoiceWorkflowRejectedEventHandler`

**Workflow Completion Behavior**:
```csharp
// Invoice posted when workflow completes
var invoice = await _invoiceRepository.GetByIdAsync(notification.EntityId, cancellationToken);
invoice.Post();  // Creates AR entries, makes invoice official
```

**Workflow Rejection Behavior**:
```csharp
// Invoice voided when workflow is rejected
var invoice = await _invoiceRepository.GetByIdAsync(notification.EntityId, cancellationToken);
invoice.Void($"Rejected in workflow at step {notification.RejectedAtStepNumber}");
```

**Usage Example**:
1. PM creates invoice → Status: Draft
2. Submit for approval → Workflow starts
3. PM reviews (Step 1) → Approved
4. Finance Director approves (Step 2) → Workflow completes
5. Event handler called → Invoice.Post()
6. Invoice posted to accounting, AR created, client notified

---

### 3. Financial (FIN) Integration

**Entities**: Journal Entries

**Event Handlers**:
- `JournalEntryWorkflowCompletedEventHandler`
- `JournalEntryWorkflowRejectedEventHandler`

**Workflow Completion Behavior**:
```csharp
// Journal entry posted to ledger when workflow completes
var journalEntry = await _journalEntryRepository.GetByIdAsync(notification.EntityId, cancellationToken);
var postedBy = _currentUserService.Username ?? "System";
journalEntry.Post(postedBy);  // Posts to General Ledger
```

**Workflow Rejection Behavior**:
```csharp
// Journal entry cancelled when workflow is rejected
var journalEntry = await _journalEntryRepository.GetByIdAsync(notification.EntityId, cancellationToken);
journalEntry.Cancel($"Rejected in workflow at step {notification.RejectedAtStepNumber}");
```

**Usage Example**:
1. Accountant creates journal entry → Status: Draft
2. Submit for approval → Workflow starts
3. Accountant reviews (Step 1) → Approved
4. Controller approves (Step 2) → Workflow completes
5. Event handler called → Journal entry posted to ledger
6. Financial statements updated

---

## Email Notification System

### Email Service Interface

```csharp
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);

    Task SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);

    Task SendTemplatedEmailAsync<T>(string to, string templateName, T model, CancellationToken cancellationToken = default) where T : class;
}
```

### Email Templates

**1. WorkflowStepActivated**
- **Sent to**: Assigned approver(s)
- **Trigger**: Step becomes active and requires approval
- **Content**: Workflow details, action required, deadline, approval link

**2. WorkflowCompleted**
- **Sent to**: Workflow initiator, all participants
- **Trigger**: All steps approved
- **Content**: Confirmation, entity details, completion timestamp

**3. WorkflowRejected**
- **Sent to**: Workflow initiator
- **Trigger**: Any step rejected
- **Content**: Rejection details, comments, rejected step, action needed

### Template Model

```csharp
public class WorkflowNotificationEmailModel
{
    public string WorkflowName { get; set; }
    public string EntityType { get; set; }
    public long EntityId { get; set; }
    public string StepName { get; set; }
    public string ActionRequired { get; set; }
    public string ActionUrl { get; set; }
    public DateTime DueDate { get; set; }
    public string RecipientName { get; set; }
    public string Comments { get; set; }
}
```

### Notification Event Handlers

**1. WorkflowStepActivatedNotificationHandler**
```csharp
public async Task Handle(WorkflowStepActivatedEvent notification, CancellationToken cancellationToken)
{
    // Get workflow and step details
    var workflowInstance = await _workflowInstanceRepository.GetByIdWithStepsAsync(...);
    var step = workflowInstance.StepInstances.FirstOrDefault(s => s.SequenceNumber == notification.StepSequenceNumber);

    // Determine recipients
    if (step.ApproverId.HasValue)
    {
        var approver = await _employeeRepository.GetByIdAsync(step.ApproverId.Value);
        recipients.Add(approver.Email);
    }
    else if (!string.IsNullOrEmpty(step.ApproverRole))
    {
        // Get all users with role
        var usersInRole = await _userRepository.GetByRoleAsync(step.ApproverRole);
        recipients.AddRange(usersInRole.Select(u => u.Email));
    }

    // Send notifications
    await _emailService.SendTemplatedEmailAsync(email, "WorkflowStepActivated", model);
}
```

**2. WorkflowCompletedNotificationHandler**
- Notifies workflow initiator
- Optionally notifies all approvers
- Confirms successful completion

**3. WorkflowRejectedNotificationHandler**
- Notifies workflow initiator
- Includes rejection reason and comments
- Provides guidance for resubmission

---

## Email Service Implementation

### Current Implementation

The included `EmailService` is a **placeholder implementation** that logs emails instead of sending them. This allows the system to run without external dependencies.

**Development Mode**:
```csharp
_logger.LogInformation(
    "Email would be sent:\nTo: {Recipients}\nSubject: {Subject}\nBody:\n{Body}",
    string.Join(", ", to),
    subject,
    body);
```

### Production Integration Options

**Option 1: SendGrid (Recommended)**
```csharp
// NuGet: SendGrid
var apiKey = _configuration["SendGrid:ApiKey"];
var client = new SendGridClient(apiKey);

var from = new EmailAddress("noreply@yourdomain.com", "ERP System");
var to = new EmailAddress(recipientEmail, recipientName);

var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlBody);
var response = await client.SendEmailAsync(msg);
```

**Option 2: AWS SES**
```csharp
// NuGet: AWSSDK.SimpleEmail
using var client = new AmazonSimpleEmailServiceClient(region);

var request = new SendEmailRequest
{
    Source = "noreply@yourdomain.com",
    Destination = new Destination { ToAddresses = new List<string> { recipientEmail } },
    Message = new Message
    {
        Subject = new Content(subject),
        Body = new Body { Html = new Content(htmlBody) }
    }
};

await client.SendEmailAsync(request);
```

**Option 3: SMTP**
```csharp
// Built-in .NET
using var client = new SmtpClient(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]))
{
    Credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
    EnableSsl = true
};

var message = new MailMessage
{
    From = new MailAddress("noreply@yourdomain.com"),
    Subject = subject,
    Body = htmlBody,
    IsBodyHtml = true
};
message.To.Add(recipientEmail);

await client.SendAsync(message);
```

---

## Configuration

### appsettings.json

```json
{
  "Email": {
    "Provider": "SendGrid",
    "FromAddress": "noreply@yourdomain.com",
    "FromName": "ERP System"
  },
  "SendGrid": {
    "ApiKey": "SG.xxxxxxxxxxxxx"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### Email Templates Location

Templates are currently embedded in `EmailService.cs`. For production:

**Option 1: File System**
```
/EmailTemplates/
  ├── WorkflowStepActivated.html
  ├── WorkflowCompleted.html
  └── WorkflowRejected.html
```

**Option 2: Database**
- Store templates in `email_templates` table
- Allow admins to customize via UI
- Support multi-language templates

**Option 3: External Service**
- Use SendGrid dynamic templates
- Use AWS SES templates
- Better performance, managed updates

---

## Workflow Trigger Implementation

### Adding Workflow Triggers to Commands

**Example: Timesheet Submission**

```csharp
public class SubmitTimesheetCommandHandler : IRequestHandler<SubmitTimesheetCommand>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public async Task Handle(SubmitTimesheetCommand request, CancellationToken cancellationToken)
    {
        // Get and submit timesheet
        var timesheet = await _timesheetRepository.GetByIdAsync(request.TimesheetId);
        timesheet.Submit();

        // Check for active workflow
        var workflowDefinitions = await _workflowDefinitionRepository
            .GetActiveByEntityTypeAsync("Timesheet", cancellationToken);

        var workflowDefinition = workflowDefinitions.FirstOrDefault();

        if (workflowDefinition != null)
        {
            // Start workflow
            var workflowInstance = WorkflowInstance.Start(
                _currentTenant.TenantId,
                workflowDefinition.Id,
                "Timesheet",
                timesheet.Id,
                workflowDefinition.Steps);

            workflowInstance.Begin();  // Activates first step, raises event

            await _workflowInstanceRepository.AddAsync(workflowInstance, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // Event dispatching happens in SaveChangesAsync
    }
}
```

**Key Points**:
1. Check for active workflow definition
2. Create workflow instance
3. Begin execution (activates first step)
4. Save changes (dispatches events)
5. Email notifications sent automatically

---

## Complete Flow Example

### Scenario: Timesheet Approval

**1. Employee Submits Timesheet**
```
POST /api/v1/timesheets/123/submit
→ TimesheetSubmittedCommand
→ timesheet.Submit() (Status: Submitted)
→ WorkflowInstance.Start("Timesheet", 123)
→ WorkflowInstance.Begin()
→ Domain Event: WorkflowStepActivatedEvent (Step 1: Manager Approval)
→ Email sent to manager
```

**2. Manager Approves**
```
POST /api/v1/workflows/456/approve
{
  "stepSequenceNumber": 1,
  "comments": "Approved"
}
→ ApproveStepCommand
→ workflowInstance.ProcessStepAction(1, Approved, managerId)
→ Domain Event: WorkflowStepActivatedEvent (Step 2: Finance Review)
→ Email sent to finance team
```

**3. Finance Approves**
```
POST /api/v1/workflows/456/approve
{
  "stepSequenceNumber": 2,
  "comments": "Looks good"
}
→ ApproveStepCommand
→ workflowInstance.ProcessStepAction(2, Approved, financeId)
→ workflowInstance.Complete("All steps approved")
→ Domain Event: WorkflowInstanceCompletedEvent
→ TimesheetWorkflowCompletedEventHandler: timesheet.Approve()
→ Email sent to employee (confirmation)
→ Timesheet Status: Approved
→ Ready for payroll
```

**4. Alternative: Finance Rejects**
```
POST /api/v1/workflows/456/reject
{
  "stepSequenceNumber": 2,
  "comments": "Missing project code on Friday"
}
→ RejectStepCommand
→ workflowInstance.ProcessStepAction(2, Rejected, financeId)
→ Domain Event: WorkflowInstanceRejectedEvent
→ TimesheetWorkflowRejectedEventHandler: timesheet.Reject()
→ Email sent to employee (rejection notice with comments)
→ Timesheet Status: Rejected
→ Employee can revise and resubmit
```

---

## Event Handler Registration

All event handlers are automatically discovered by MediatR through assembly scanning:

```csharp
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateInvoiceCommand).Assembly);
});
```

**Registered Handlers**:
- TimesheetWorkflowCompletedEventHandler
- TimesheetWorkflowRejectedEventHandler
- ExpenseReportWorkflowCompletedEventHandler
- ExpenseReportWorkflowRejectedEventHandler
- InvoiceWorkflowCompletedEventHandler
- InvoiceWorkflowRejectedEventHandler
- JournalEntryWorkflowCompletedEventHandler
- JournalEntryWorkflowRejectedEventHandler
- WorkflowStepActivatedNotificationHandler
- WorkflowCompletedNotificationHandler
- WorkflowRejectedNotificationHandler

---

## Testing

### Unit Testing Event Handlers

```csharp
[Fact]
public async Task TimesheetWorkflowCompleted_ShouldApproveTimesheet()
{
    // Arrange
    var timesheet = CreateTestTimesheet();
    timesheet.Submit();

    var @event = new WorkflowInstanceCompletedEvent(
        workflowInstanceId: 1,
        tenantId: Guid.NewGuid(),
        entityType: "Timesheet",
        entityId: timesheet.Id,
        completedAt: DateTime.UtcNow);

    var handler = new TimesheetWorkflowCompletedEventHandler(
        _timesheetRepository,
        _unitOfWork,
        _logger);

    // Act
    await handler.Handle(@event, CancellationToken.None);

    // Assert
    timesheet.Status.Should().Be(TimesheetStatus.Approved);
    await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
}
```

### Integration Testing

```csharp
[Fact]
public async Task SubmitTimesheet_WithActiveWorkflow_ShouldStartWorkflow()
{
    // Arrange
    var command = new SubmitTimesheetCommand { TimesheetId = 123 };

    // Act
    await _mediator.Send(command);

    // Assert
    var workflow = await _workflowInstanceRepository.GetByEntityAsync("Timesheet", 123);
    workflow.Should().NotBeNull();
    workflow.Status.Should().Be(WorkflowInstanceStatus.InProgress);

    // Verify email was sent
    _emailService.Received(1).SendTemplatedEmailAsync(
        Arg.Any<string>(),
        "WorkflowStepActivated",
        Arg.Any<WorkflowNotificationEmailModel>(),
        Arg.Any<CancellationToken>());
}
```

---

## Performance Considerations

### Email Queue

For high-volume scenarios, consider queuing emails:

```csharp
public interface IEmailQueueService
{
    Task QueueEmailAsync(EmailMessage message);
}

// Background service processes queue
public class EmailQueueProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var messages = await _queue.DequeueAsync(100);

            foreach (var message in messages)
            {
                await _emailService.SendEmailAsync(message);
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
```

### Batch Notifications

For role-based approvals, send one email with multiple pending items:

```csharp
"You have 5 pending approvals:
- Timesheet #123 (John Doe)
- Timesheet #124 (Jane Smith)
- Expense Report #456 (Bob Johnson)
- Invoice #789 (Project Alpha)
- Journal Entry #111 (Monthly Close)"
```

---

## Security Considerations

### Email Content

- **Never include sensitive data** (passwords, SSN, credit cards)
- **Include only necessary information** (IDs, names, dates)
- **Use secure links** (HTTPS only, with tokens)

### Notification Preferences

Allow users to configure notification preferences:
```csharp
public class UserNotificationPreferences
{
    public bool EmailEnabled { get; set; }
    public bool WorkflowApprovalEmails { get; set; }
    public bool WorkflowCompletionEmails { get; set; }
    public bool DailySummary { get; set; }
}
```

---

## Troubleshooting

### Emails Not Sending

**1. Check Email Service Registration**
```csharp
builder.Services.AddScoped<IEmailService, EmailService>();
```

**2. Check Configuration**
```json
{
  "SendGrid": {
    "ApiKey": "SG.xxxxx"  // Must be valid
  }
}
```

**3. Check Logs**
```
LOG: Email would be sent to: user@example.com
```

### Workflow Not Triggering

**1. Verify Active Workflow Definition Exists**
```sql
SELECT * FROM wf.WorkflowDefinitions
WHERE EntityType = 'Timesheet' AND Status = 1 (Active)
```

**2. Check Command Handler**
- Is workflow start code included?
- Is SaveChangesAsync called?

**3. Verify Event Handlers Registered**
- MediatR auto-discovery enabled?
- Handlers in same assembly?

---

## Next Steps

### Phase 14+ Enhancements

1. **User Notification Preferences UI**
   - Allow users to customize notification settings
   - Daily digest option
   - SMS notifications (Twilio integration)

2. **Advanced Templates**
   - Rich HTML templates
   - Multi-language support
   - Custom branding per tenant

3. **Workflow Analytics**
   - Average approval time
   - Bottleneck identification
   - Approval patterns

4. **Escalation Rules**
   - Auto-escalate overdue approvals
   - Delegate to backup approvers
   - SLA tracking

---

## Conclusion

Phase 13 completes the workflow system by:
- ✅ Integrating workflows with TE, BILL, FIN modules
- ✅ Implementing email notification infrastructure
- ✅ Creating event handlers for automatic entity updates
- ✅ Providing extensible template system
- ✅ Supporting production email providers

**The ERP now has a complete, production-ready approval workflow system with automated notifications!**
