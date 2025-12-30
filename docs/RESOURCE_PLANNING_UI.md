# Resource Planning Module - UI Specifications

## Overview

The Resource Planning module provides comprehensive project scheduling, resource allocation, and capacity management through visual interfaces including Gantt charts, resource grids, and capacity heatmaps.

---

## 1. Project Schedule View (Gantt Chart)

### Purpose
Visualize and manage project work breakdown structure timeline with dependencies, resource assignments, and progress tracking.

### Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Project Schedule: Project XYZ                            [Today] [Options]  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│ ┌─────────────────────┬───────────────────────────────────────────────────┐ │
│ │  WBS Structure      │         Timeline (Gantt Chart)                    │ │
│ ├─────────────────────┼───────────────────────────────────────────────────┤ │
│ │                     │  Jan  │  Feb  │  Mar  │  Apr  │  May  │  Jun  │   │ │
│ ├─────────────────────┼───────────────────────────────────────────────────┤ │
│ │ ▼ 1. Project Phase  │ ████████████████████████████████████              │ │
│ │   ▼ 1.1 Design      │   ██████████                                      │ │
│ │     1.1.1 UI/UX     │   ████                                            │ │
│ │     1.1.2 Architecture│    ████                                         │ │
│ │   ▼ 1.2 Development │           ████████████████                        │ │
│ │     1.2.1 Backend   │           ████████                                │ │
│ │     1.2.2 Frontend  │                 ████████                          │ │
│ └─────────────────────┴───────────────────────────────────────────────────┘ │
│                                                                              │
│ Selected: 1.2.1 Backend Development                                         │
│ Duration: Feb 15 - Mar 15 (4 weeks) │ Est: 160 hrs │ Actual: 45 hrs        │
│ Resources: John Smith (80h), Jane Doe (80h)                                 │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Features

#### Gantt Chart Display
- **Timeline Scale**: Day/Week/Month views with zoom controls
- **Task Bars**:
  - Color-coded by status (Not Started: gray, In Progress: blue, Completed: green, On Hold: yellow)
  - Progress indicator within bar showing % complete
  - Hover shows details: dates, hours, resources
- **Dependencies**:
  - Visual arrows between dependent tasks
  - Types: Finish-to-Start (FS), Start-to-Start (SS), Finish-to-Finish (FF), Start-to-Finish (SF)
  - Lag/lead indicators
- **Critical Path**: Highlight tasks on critical path in red
- **Today Line**: Vertical line showing current date

#### WBS Tree (Left Panel)
- **Hierarchical Display**:
  - Level 1: Project (bold, blue)
  - Level 2: Phase (indented, medium weight)
  - Level 3: Task (indented more, normal weight)
- **Expand/Collapse**: Click arrow icons to show/hide children
- **WBS Codes**: Display codes (e.g., "1.2.1") before names
- **Status Icons**: Visual indicators for each item's status
- **Progress Bars**: Small progress bar showing % complete

#### Interaction Features
- **Drag-and-Drop**:
  - Drag task bars to change start/end dates
  - Drag bar edges to resize duration
  - Auto-recalculate dependent tasks
- **Double-Click**: Open task detail modal
- **Right-Click Menu**:
  - Add Dependency
  - Assign Resources
  - Edit Task
  - Delete Task
  - Add Subtask
- **Multi-Select**: Shift+click or Ctrl+click to select multiple tasks
- **Bulk Operations**: Move, reschedule, or assign resources to multiple tasks

#### Toolbar Actions
- **View Options**:
  - [ ] Show Dependencies
  - [ ] Show Critical Path
  - [ ] Show Resource Names
  - [ ] Show Progress
- **Level Filter**: Filter by WBS level (All, Project, Phase, Task)
- **Export**: Export to PDF, PNG, Excel, MS Project XML
- **Print**: Print Gantt chart
- **Auto-Schedule**: Automatically schedule based on dependencies and resource availability

---

## 2. Resource Assignment Interface

### Purpose
Assign employees or generic resources to WBS items with flexible allocation methods.

### Layout Options

#### Option A: Grid View

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Resource Assignments: 1.2.1 Backend Development                             │
│ Duration: Feb 15 - Mar 15, 2025 (4 weeks)                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│ Assignment Type: ○ Total Hours  ● Hours/Week  ○ Budget                      │
│                                                                              │
│ ┌────────────────────────────────────────────────────────────────────────┐  │
│ │ Resource          │ Type        │ Hours/Week │ Total │ Rate   │ Cost   │  │
│ ├────────────────────────────────────────────────────────────────────────┤  │
│ │ John Smith        │ Sr Dev      │ 20         │ 80    │ $150   │ $12k   │  │
│ │ Jane Doe          │ Jr Dev      │ 20         │ 80    │ $100   │ $8k    │  │
│ │ [+ Assign]        │             │            │       │        │        │  │
│ └────────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
│ Total Hours: 160 │ Total Cost: $20,000 │ Budget: $25,000 │ Remaining: $5k  │
│                                                                              │
│ [Add Employee] [Add Generic Resource] [Save] [Cancel]                       │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### Option B: Drag-and-Drop from Resource Pool

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Resource Assignment                                                          │
├──────────────────────────────┬──────────────────────────────────────────────┤
│ Available Resources          │ Assigned to: 1.2.1 Backend Development       │
│ [Search: ____________]       │                                               │
│                              │ Duration: Feb 15 - Mar 15 (4 weeks)           │
│ ┌──────────────────────────┐ │                                               │
│ │ 👤 John Smith            │ │ ┌──────────────────────────────────────────┐ │
│ │    Sr Developer          │ │ │ 👤 Jane Doe                              │ │
│ │    Avail: 75%           │ │ │    Jr Developer                           │ │
│ │    [Drag to assign →]    │ │ │    ┌──────────────────────────────────┐ │ │
│ │                          │ │ │    │ Assignment Type: Hours/Week       │ │ │
│ │ 👤 Jane Doe              │ │ │    │ Hours: [20___] hrs/week          │ │ │
│ │    Jr Developer         │ │ │    │ Total: 80 hrs │ Cost: $8,000     │ │ │
│ │    Avail: 100%          │ │ │    │ [×Remove]                        │ │ │
│ │    [Drag to assign →]    │ │ │    └──────────────────────────────────┘ │ │
│ │                          │ │ └──────────────────────────────────────────┘ │
│ │ 📋 Senior Developer      │ │                                               │
│ │    (Generic)            │ │ Total: 80 hrs │ $8,000                        │
│ │    [Drag to assign →]    │ │                                               │
│ └──────────────────────────┘ │ [Save] [Cancel]                               │
└──────────────────────────────┴──────────────────────────────────────────────┘
```

### Assignment Type Configuration

#### Total Hours Assignment
```
┌─────────────────────────────────────────┐
│ Assignment Type: Total Hours            │
├─────────────────────────────────────────┤
│ Total Hours: [160_____] hours           │
│                                         │
│ Distribution: Auto calculated           │
│ • Start: Feb 15, 2025                   │
│ • End: Mar 15, 2025                     │
│ • Weeks: 4                              │
│ • Hours/Week: 40                        │
│                                         │
│ Allocation: [100__]% of resource time   │
│                                         │
│ Rates:                                  │
│ • Cost Rate: $125/hr                    │
│ • Billing Rate: $175/hr                 │
│                                         │
│ Calculated Cost: $20,000                │
│ Calculated Revenue: $28,000             │
│                                         │
│ [Assign] [Cancel]                       │
└─────────────────────────────────────────┘
```

#### Hours Per Week Assignment
```
┌─────────────────────────────────────────┐
│ Assignment Type: Hours Per Week         │
├─────────────────────────────────────────┤
│ Hours/Week: [20_____] hours             │
│                                         │
│ Duration:                               │
│ • Start: Feb 15, 2025                   │
│ • End: Mar 15, 2025                     │
│ • Weeks: 4                              │
│                                         │
│ Calculated Total: 80 hours              │
│                                         │
│ Allocation: [50__]% of resource time    │
│ (Remaining capacity: 20 hrs/week)       │
│                                         │
│ Rates:                                  │
│ • Cost Rate: $125/hr                    │
│ • Billing Rate: $175/hr                 │
│                                         │
│ Calculated Cost: $10,000                │
│ Calculated Revenue: $14,000             │
│                                         │
│ [Assign] [Cancel]                       │
└─────────────────────────────────────────┘
```

#### Budget Assignment
```
┌─────────────────────────────────────────┐
│ Assignment Type: Budget                 │
├─────────────────────────────────────────┤
│ Budget Amount: $[10,000__]              │
│                                         │
│ Billing Rate: $175/hr                   │
│                                         │
│ Calculated Hours: 57.14 hours           │
│                                         │
│ Duration:                               │
│ • Start: Feb 15, 2025                   │
│ • End: Mar 15, 2025                     │
│ • Weeks: 4                              │
│                                         │
│ Distribution: 14.3 hrs/week             │
│                                         │
│ Allocation: [36__]% of resource time    │
│                                         │
│ Estimated Cost: $7,142.50               │
│ Margin: $2,857.50 (28.6%)               │
│                                         │
│ [Assign] [Cancel]                       │
└─────────────────────────────────────────┘
```

### Generic Resource Assignment

```
┌─────────────────────────────────────────┐
│ Add Generic Resource                    │
├─────────────────────────────────────────┤
│ Resource Type:                          │
│ [▼ Senior Developer_______]             │
│                                         │
│ Or Custom Name:                         │
│ [________________________]              │
│                                         │
│ Default Rates (from type):              │
│ • Cost Rate: $125/hr                    │
│ • Billing Rate: $175/hr                 │
│                                         │
│ Assignment:                             │
│ Type: [▼ Hours Per Week____]            │
│ Value: [20_____] hrs/week               │
│                                         │
│ Duration:                               │
│ Start: [02/15/2025]                     │
│ End: [03/15/2025]                       │
│                                         │
│ Total: 80 hrs │ $10,000                 │
│                                         │
│ [✓] Mark as "To Be Hired"               │
│                                         │
│ [Add] [Cancel]                          │
└─────────────────────────────────────────┘
```

---

## 3. Resource Capacity View

### Purpose
Visual overview of resource utilization, availability, and capacity across all projects.

### Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Resource Capacity Planning                                    [This Month ▼]│
├─────────────────────────────────────────────────────────────────────────────┤
│ Filter: [All Departments ▼] [All Roles ▼] [🔍 Search...]                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│ ┌────────────────────────────────────────────────────────────────────────┐  │
│ │ Resource      │ Week 1  │ Week 2  │ Week 3  │ Week 4  │ Avg Util │     │  │
│ │               │ 2/5-2/9 │2/12-2/16│2/19-2/23│2/26-3/1 │          │     │  │
│ ├────────────────────────────────────────────────────────────────────────┤  │
│ │ John Smith    │ ██████  │ ████    │ ███████ │ ███████ │ 88%      │ ⚠️  │  │
│ │ Sr Developer  │ 35/40   │ 25/40   │ 38/40   │ 38/40   │          │     │  │
│ │               │         │         │         │         │          │     │  │
│ │ Jane Doe      │ ████    │ ████    │ ████    │ ████    │ 50%      │ ✓   │  │
│ │ Jr Developer  │ 20/40   │ 20/40   │ 20/40   │ 20/40   │          │     │  │
│ │               │         │         │         │         │          │     │  │
│ │ Bob Wilson    │ ████████│ ████████│ ████████│ ███     │ 98%      │ 🔴  │  │
│ │ Sr Developer  │ 42/40   │ 45/40   │ 40/40   │ 30/40   │          │     │  │
│ │               │         │         │         │         │          │     │  │
│ │ Sarah Lee     │         │ █       │ █       │ █       │ 8%       │ ✓   │  │
│ │ Designer      │ 0/40    │ 3/40    │ 3/40    │ 3/40    │          │     │  │
│ └────────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
│ Legend: ▓▓▓▓ 0-75% │ ████ 76-100% │ ████ 101-125% │ ████ >125%             │
│         Green      │ Blue         │ Orange       │ Red                      │
│                                                                              │
│ Click on a cell to see project breakdown                                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Capacity Detail Modal (Click on a cell)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Bob Wilson - Week of Feb 12-16, 2025                                [Close] │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│ Total Allocated: 45 hours │ Available: 40 hours │ Overallocated: 5 hours   │
│ Utilization: 112.5% 🔴                                                       │
│                                                                              │
│ ┌──────────────────────────────────────────────────────────────────────┐    │
│ │ Project          │ WBS Item         │ Hours │ Status      │ Action  │    │
│ ├──────────────────────────────────────────────────────────────────────┤    │
│ │ Project Alpha    │ 1.2.1 Backend    │ 20    │ In Progress │ [View]  │    │
│ │ Project Beta     │ 2.1 Design       │ 15    │ Confirmed   │ [View]  │    │
│ │ Project Gamma    │ 3.3 Testing      │ 10    │ Planned     │ [View]  │    │
│ └──────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│ ⚠️ Warning: This resource is overallocated by 5 hours this week.            │
│                                                                              │
│ Suggestions:                                                                 │
│ • Reduce allocation on Project Gamma (lowest priority)                      │
│ • Reassign 5 hours to available team member                                 │
│ • Extend timeline for one of the projects                                   │
│                                                                              │
│ [Optimize Allocation] [Contact Manager] [Export Report]                     │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Capacity Heatmap View

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Resource Heatmap - Q1 2025                                    [Quarter ▼]   │
├─────────────────────────────────────────────────────────────────────────────┤
│                 Jan                 Feb                 Mar                  │
│              1 2 3 4 5          1 2 3 4 5          1 2 3 4 5                │
│ ┌──────────────────────────────────────────────────────────────────────┐    │
│ │ John Smith  █ █ █ ▓ ▓          █ █ ▓ █ █          ▓ ▓ ▓ ▓ █          │    │
│ │ Jane Doe    ▓ ▓ ▓ ▓ ▓          ▓ ▓ ▓ ▓ ▓          ▓ ▓ ▓ ▓ ▓          │    │
│ │ Bob Wilson  █ █ █ █ █          █ █ █ █ █          █ █ █ █ ▓          │    │
│ │ Sarah Lee   □ □ □ □ ▓          ▓ ▓ ▓ ▓ ▓          █ █ █ █ █          │    │
│ │ Mike Chen   █ █ █ █ █          █ █ █ ▓ ▓          ▓ ▓ ▓ ▓ ▓          │    │
│ └──────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│ Legend: □ <25% │ ▓ 25-75% │ █ 76-100% │ █ 101-125% │ █ >125%              │
│                                                                              │
│ Click on any cell to see weekly details                                     │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Resource Planning Dashboard

### Purpose
Executive overview of resource utilization, project status, and capacity alerts.

### Layout

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Resource Planning Dashboard                                 [Refresh] [⚙️]  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│ ┌─────────────────────┬────────────────────┬─────────────────────────────┐  │
│ │ Team Utilization    │ Available Capacity │ Overallocated Resources    │  │
│ │                     │                    │                             │  │
│ │      76%            │   320 hrs/week     │        3                    │  │
│ │   ┌─────────┐       │   (8 resources)    │                             │  │
│ │   │    ██   │       │                    │   ⚠️ Needs Attention        │  │
│ │   │   ████  │       │   Underutilized: 2 │                             │  │
│ │   │  ██████ │       │   Well-utilized: 5 │                             │  │
│ │   │ ████████│       │   Overallocated: 3 │                             │  │
│ │   └─────────┘       │                    │                             │  │
│ └─────────────────────┴────────────────────┴─────────────────────────────┘  │
│                                                                              │
│ ┌──────────────────────────────────────────────────────────────────────┐    │
│ │ Alerts & Warnings                                                    │    │
│ ├──────────────────────────────────────────────────────────────────────┤    │
│ │ 🔴 Bob Wilson overallocated by 15 hrs in Week 2/12                   │    │
│ │ 🔴 Mike Chen overallocated by 8 hrs in Week 2/19                     │    │
│ │ ⚠️ Project Alpha missing 40 hrs of Senior Dev capacity               │    │
│ │ ⚠️ 3 generic resources need to be assigned to actual employees       │    │
│ │ ℹ️ Sarah Lee available for 35 hrs/week                               │    │
│ └──────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│ ┌──────────────────────────────────────────────────────────────────────┐    │
│ │ Project Timeline Status                                              │    │
│ ├──────────────────────────────────────────────────────────────────────┤    │
│ │ Project          │ Status      │ % Complete │ On Track │ Resources  │    │
│ ├──────────────────────────────────────────────────────────────────────┤    │
│ │ Project Alpha    │ In Progress │ 45%        │ ✓ Yes    │ 4          │    │
│ │ Project Beta     │ In Progress │ 30%        │ ⚠️ Risk  │ 3          │    │
│ │ Project Gamma    │ Planning    │ 5%         │ ✓ Yes    │ 2          │    │
│ │ Project Delta    │ Not Started │ 0%         │ ⚠️ Delay │ 0          │    │
│ └──────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│ ┌────────────────────────────────┬─────────────────────────────────────┐    │
│ │ Top Skills in Demand           │ Hiring Pipeline                     │    │
│ ├────────────────────────────────┼─────────────────────────────────────┤    │
│ │ 1. Senior Developer (120 hrs)  │ • 2 Senior Devs (In Process)        │    │
│ │ 2. DevOps Engineer (80 hrs)    │ • 1 Designer (Offer Extended)       │    │
│ │ 3. UI/UX Designer (60 hrs)     │ • 1 QA Engineer (Interviews)        │    │
│ │ 4. QA Engineer (40 hrs)        │                                     │    │
│ └────────────────────────────────┴─────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Technical Implementation

### Frontend Components (Tailwind CSS + JavaScript)

#### Gantt Chart Component
**Technology**:
- DHTMLX Gantt (Commercial) or
- Bryntum Gantt (Commercial) or
- FrappeGantt (Open Source)

**Features to Implement**:
- Real-time updates via SignalR
- Auto-save on drag operations
- Undo/Redo support
- Keyboard shortcuts
- Responsive design for tablets

#### Resource Grid Component
**Technology**: AG-Grid (with row grouping and editing)

**Features**:
- Inline editing
- Drag-and-drop from resource pool
- Cell validation
- Conditional formatting based on capacity

#### Capacity Heatmap
**Technology**: Custom D3.js or Chart.js implementation

**Features**:
- Color gradients for utilization levels
- Interactive tooltips
- Click-through to detailed view
- Export to image

### API Endpoints

```csharp
// WBS Scheduling
POST   /api/v1/projects/{projectId}/wbs/{wbsId}/schedule
PUT    /api/v1/projects/{projectId}/wbs/{wbsId}/schedule
GET    /api/v1/projects/{projectId}/gantt-data

// Dependencies
POST   /api/v1/projects/{projectId}/wbs/{wbsId}/dependencies
DELETE /api/v1/projects/{projectId}/wbs/dependencies/{dependencyId}
GET    /api/v1/projects/{projectId}/wbs/{wbsId}/dependencies

// Resource Assignments
POST   /api/v1/projects/{projectId}/wbs/{wbsId}/assignments
PUT    /api/v1/projects/{projectId}/wbs/assignments/{assignmentId}
DELETE /api/v1/projects/{projectId}/wbs/assignments/{assignmentId}
GET    /api/v1/projects/{projectId}/wbs/{wbsId}/assignments

// Convert Generic to Employee
PUT    /api/v1/projects/{projectId}/wbs/assignments/{assignmentId}/convert-to-employee

// Capacity Management
GET    /api/v1/resources/capacity?startDate={date}&endDate={date}
GET    /api/v1/resources/{employeeId}/capacity?startDate={date}&endDate={date}
GET    /api/v1/resources/capacity/alerts
POST   /api/v1/resources/capacity/recalculate

// Resource Pool
GET    /api/v1/resources/available?startDate={date}&endDate={date}&skillId={id}
GET    /api/v1/resources/{employeeId}/assignments
```

### Real-time Features (SignalR)

```csharp
// Hub Methods
public class ResourcePlanningHub : Hub
{
    // Notify when resource is assigned
    Task OnResourceAssigned(long projectId, long wbsItemId, long assignmentId);

    // Notify when schedule changes
    Task OnScheduleChanged(long projectId, long wbsItemId);

    // Notify when resource becomes overallocated
    Task OnResourceOverallocated(long employeeId, DateTime weekStartDate);

    // Notify when dependency is added
    Task OnDependencyAdded(long wbsItemId, long dependencyId);
}
```

---

## 6. User Workflows

### Workflow 1: Schedule Project with Resources

1. Navigate to Project → Resource Planning
2. View Gantt chart with existing WBS structure
3. Click on WBS item "Backend Development"
4. Drag task bar to set dates: Feb 15 - Mar 15
5. Click "Assign Resources" button
6. Select assignment type: "Hours Per Week"
7. Drag "John Smith" from resource pool
8. Set hours: 20 hrs/week
9. System shows:
   - Total: 80 hours
   - Cost: $10,000
   - John's utilization increases to 88%
   - ⚠️ Warning: John near capacity
10. Add another resource "Jane Doe" with 20 hrs/week
11. Save assignment
12. System automatically:
    - Updates capacity calculations
    - Sends notifications to resources
    - Updates project budget tracking

### Workflow 2: Resolve Overallocation

1. View Capacity Dashboard
2. See alert: "Bob Wilson overallocated by 5 hrs in Week 2/12"
3. Click on Bob's name
4. See assignments:
   - Project Alpha: 20 hrs
   - Project Beta: 15 hrs
   - Project Gamma: 10 hrs (Planned status)
5. Click "Optimize Allocation"
6. System suggests:
   - Reassign Project Gamma to Sarah Lee (available)
   - Extend Project Beta timeline by 1 week
7. Select "Reassign to Sarah Lee"
8. System:
   - Removes Bob from Project Gamma
   - Assigns Sarah to Project Gamma
   - Recalculates capacity
   - Clears overallocation alert

### Workflow 3: Convert Generic to Employee

1. View project assignments
2. See generic resource: "Senior Developer TBH"
3. Click "Convert to Employee"
4. Select from dropdown: "John Smith"
5. System:
   - Checks John's capacity for the period
   - Shows capacity impact
   - Warns if overallocated
6. Confirm assignment
7. System:
   - Updates assignment with John's actual rates
   - Recalculates budget
   - Updates capacity
   - Sends notification to John

---

## 7. Visual Design Guidelines

### Color Scheme

**Status Colors**:
- Not Started: Gray (#9CA3AF)
- In Progress: Blue (#3B82F6)
- Completed: Green (#10B981)
- On Hold: Yellow (#F59E0B)
- Cancelled: Red (#EF4444)

**Capacity Colors**:
- Available (0-75%): Green (#10B981)
- Well-Utilized (76-100%): Blue (#3B82F6)
- Near Capacity (101-115%): Orange (#F97316)
- Overallocated (>115%): Red (#EF4444)

**Dependency Lines**:
- Finish-to-Start: Solid blue
- Start-to-Start: Dashed blue
- Finish-to-Finish: Dotted blue
- Start-to-Finish: Dash-dot blue

### Typography

- Headings: `text-xl font-semibold text-gray-900`
- Subheadings: `text-lg font-medium text-gray-700`
- Body: `text-sm text-gray-600`
- Labels: `text-xs font-medium text-gray-500`
- Numbers: `font-mono text-sm`

### Responsive Breakpoints

- **Mobile** (< 640px): Hide Gantt, show list view only
- **Tablet** (640px - 1024px): Simplified Gantt with basic features
- **Desktop** (> 1024px): Full Gantt with all features

---

## 8. Performance Considerations

- **Lazy Loading**: Load only visible timeframe in Gantt
- **Virtual Scrolling**: For resource lists with 100+ employees
- **Debounced Calculations**: Wait 500ms after drag before recalculating
- **Cached Capacity Data**: Cache per-week capacity for 5 minutes
- **Background Jobs**: Recalculate all capacity nightly
- **Optimistic UI Updates**: Update UI immediately, sync with server asynchronously

---

## 9. Accessibility

- **Keyboard Navigation**: Tab through tasks, arrow keys to expand/collapse
- **Screen Readers**: ARIA labels for all interactive elements
- **Color Contrast**: WCAG AA compliant (4.5:1 minimum)
- **Focus Indicators**: Clear focus rings on interactive elements
- **Alternative Text**: Describe visual capacity indicators in text

---

## 10. Future Enhancements

- **AI-Powered Scheduling**: Auto-suggest optimal resource allocation
- **What-If Scenarios**: Create multiple scheduling scenarios
- **Skills-Based Matching**: Suggest resources based on required skills
- **Resource Request Workflow**: Request resources from managers
- **Mobile App**: Native iOS/Android for timesheet entry
- **Integration**: Sync with MS Project, Jira, Monday.com
- **Predictive Analytics**: Predict project delays based on resource patterns
- **Resource Marketplace**: Share resources across departments/projects

---

This comprehensive UI specification provides a complete blueprint for implementing the Resource Planning module with professional-grade user experience and functionality.
