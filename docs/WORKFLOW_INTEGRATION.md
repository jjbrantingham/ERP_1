# Workflow Engine Integration Guide

## Overview

The Workflow Engine (WF module) provides a configurable approval system that can be integrated with any entity in the ERP system. This guide shows how to integrate workflows with Time & Expense (TE), Billing (BILL), and Financial (FIN) modules.

## Integration Pattern

### 1. Create Workflow Definition

First, create a workflow definition for the entity type:

```csharp
// Example: Create approval workflow for Timesheets
var workflowDefinition = WorkflowDefinition.Create(
    tenantId,
    "Timesheet Approval",
    "Two-step approval for timesheets",
    "Timesheet");

// Add approval steps
workflowDefinition.AddStep(
    "Manager Approval",
    "Direct manager reviews and approves timesheet",
    StepType.Approval,
    sequenceNumber: 1,
    approverRole: "Manager",
    timeoutHours: 48);

workflowDefinition.AddStep(
    "Finance Review",
    "Finance team final review",
    StepType.Approval,
    sequenceNumber: 2,
    approverRole: "Finance",
    timeoutHours: 24);

workflowDefinition.Activate();
```

### 2. Start Workflow When Entity is Submitted

When a user submits an entity for approval, start the workflow:

```csharp
// In Timesheet submission handler
public async Task Handle(SubmitTimesheetCommand request, CancellationToken cancellationToken)
{
    // Get timesheet and submit it
    var timesheet = await _timesheetRepository.GetByIdAsync(request.TimesheetId, cancellationToken);
    timesheet.Submit();

    // Get active workflow definition for Timesheets
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

        workflowInstance.Begin();

        await _workflowInstanceRepository.AddAsync(workflowInstance, cancellationToken);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

### 3. Handle Workflow Events

Subscribe to workflow domain events to update entity status:

```csharp
// Event handler for workflow completion
public class WorkflowInstanceCompletedEventHandler
    : INotificationHandler<WorkflowInstanceCompletedEvent>
{
    public async Task Handle(
        WorkflowInstanceCompletedEvent notification,
        CancellationToken cancellationToken)
    {
        // Update entity based on workflow completion
        switch (notification.EntityType)
        {
            case "Timesheet":
                var timesheet = await _timesheetRepository
                    .GetByIdAsync(notification.EntityId, cancellationToken);
                timesheet.Approve();
                break;

            case "Invoice":
                var invoice = await _invoiceRepository
                    .GetByIdAsync(notification.EntityId, cancellationToken);
                invoice.Post();
                break;

            case "JournalEntry":
                var journalEntry = await _journalEntryRepository
                    .GetByIdAsync(notification.EntityId, cancellationToken);
                journalEntry.Post("System");
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

// Event handler for workflow rejection
public class WorkflowInstanceRejectedEventHandler
    : INotificationHandler<WorkflowInstanceRejectedEvent>
{
    public async Task Handle(
        WorkflowInstanceRejectedEvent notification,
        CancellationToken cancellationToken)
    {
        // Update entity based on workflow rejection
        switch (notification.EntityType)
        {
            case "Timesheet":
                var timesheet = await _timesheetRepository
                    .GetByIdAsync(notification.EntityId, cancellationToken);
                timesheet.Reject();
                break;

            case "Invoice":
                var invoice = await _invoiceRepository
                    .GetByIdAsync(notification.EntityId, cancellationToken);
                invoice.Void("Rejected in approval workflow");
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
```

## Integration with Specific Modules

### Time & Expense (TE) Module

**Entities that need workflows:**
- Timesheets (employee submits → manager approves → finance reviews)
- Expense Reports (employee submits → manager approves → finance processes)

**Workflow Definitions:**
```
1. "Timesheet Approval"
   - Step 1: Manager Approval (role: Manager, timeout: 48h)
   - Step 2: Finance Review (role: Finance, timeout: 24h)

2. "Expense Report Approval"
   - Step 1: Manager Approval (role: Manager, timeout: 48h)
   - Step 2: Finance Review (role: Finance, timeout: 24h)
   - Step 3: Accounting Processing (role: Accountant, timeout: 72h)
```

### Billing (BILL) Module

**Entities that need workflows:**
- Invoices (draft → review → post to accounting)

**Workflow Definitions:**
```
3. "Invoice Approval"
   - Step 1: Project Manager Review (role: ProjectManager, timeout: 24h)
   - Step 2: Finance Director Approval (role: FinanceDirector, timeout: 24h)
   - Step 3: Post to Accounting (automated step)
```

### Financial (FIN) Module

**Entities that need workflows:**
- Journal Entries (created → reviewed → posted)

**Workflow Definitions:**
```
4. "Journal Entry Approval"
   - Step 1: Accountant Review (role: Accountant, timeout: 24h)
   - Step 2: Controller Approval (role: Controller, timeout: 48h)
   - Step 3: Post to Ledger (automated step)
```

## API Usage Examples

### Start a workflow for a timesheet

```http
POST /api/v1/workflows
{
  "workflowDefinitionId": 1,
  "entityType": "Timesheet",
  "entityId": 123
}
```

### Get pending approvals for current user

```http
GET /api/v1/workflows/pending-approvals
```

### Approve a workflow step

```http
POST /api/v1/workflows/5/approve
{
  "stepSequenceNumber": 1,
  "comments": "Approved - looks good"
}
```

### Reject a workflow step

```http
POST /api/v1/workflows/5/reject
{
  "stepSequenceNumber": 1,
  "comments": "Please revise hours for Project ABC"
}
```

### Check workflow status for an entity

```http
GET /api/v1/workflows/entity/Timesheet/123
```

## Database Schema

The workflow system uses the `wf` schema with the following tables:

- **wf.WorkflowDefinitions** - Template definitions
- **wf.WorkflowSteps** - Steps in each definition
- **wf.WorkflowInstances** - Runtime instances
- **wf.StepInstances** - Runtime step tracking

## Next Steps

To integrate workflows with your module:

1. Create workflow definitions in database/seed data
2. Add workflow start logic to entity submission commands
3. Create event handlers for WorkflowInstanceCompleted and WorkflowInstanceRejected
4. Update UI to show workflow status and pending approvals
5. Add "My Approvals" dashboard widget

## Configuration

Workflow definitions can be managed via:

- Database seed data (recommended for initial setup)
- Admin UI (to be implemented in Phase 12)
- REST API endpoints in WorkflowDefinitionsController

## Best Practices

1. **Always check for active workflows** before allowing manual approval
2. **Use timeout escalations** for critical approvals
3. **Add meaningful comments** when approving/rejecting
4. **Track approval history** via StepInstances
5. **Handle workflow events** to update entity status automatically

## Troubleshooting

**Workflow not starting?**
- Verify active workflow definition exists for entity type
- Check that entity is in correct status (e.g., Submitted)

**Approval button not showing?**
- Verify user has correct role or is assigned approver
- Check that step is in Active status

**Workflow stuck?**
- Check StepInstances for timeout status
- Review event handler logs for errors
