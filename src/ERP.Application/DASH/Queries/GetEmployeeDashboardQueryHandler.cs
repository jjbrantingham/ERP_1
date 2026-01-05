using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.DASH.DTOs;
using ERP.Domain.DASH.Enums;
using ERP.Domain.HR.Repositories;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using ERP.Domain.TE.Enums;
using ERP.Domain.TE.Repositories;
using MediatR;

namespace ERP.Application.DASH.Queries;

/// <summary>
/// Handler for getting employee dashboard data
/// </summary>
public class GetEmployeeDashboardQueryHandler : IRequestHandler<GetEmployeeDashboardQuery, EmployeeDashboardDto>
{
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IExpenseReportRepository _expenseReportRepository;
    private readonly IProjectRepository _projectRepository;

    public GetEmployeeDashboardQueryHandler(
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser,
        IEmployeeRepository employeeRepository,
        ITimesheetRepository timesheetRepository,
        IExpenseReportRepository expenseReportRepository,
        IProjectRepository projectRepository)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _employeeRepository = employeeRepository;
        _timesheetRepository = timesheetRepository;
        _expenseReportRepository = expenseReportRepository;
        _projectRepository = projectRepository;
    }

    public async Task<EmployeeDashboardDto> Handle(GetEmployeeDashboardQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        // Get employee (would typically get from current user context)
        var employeeId = request.EmployeeId ?? 1; // Placeholder
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
            throw new InvalidOperationException("Employee not found");

        var asOfDate = request.AsOfDate ?? DateTime.UtcNow;
        var currentWeekStart = asOfDate.AddDays(-(int)asOfDate.DayOfWeek);
        var currentMonthStart = new DateTime(asOfDate.Year, asOfDate.Month, 1);

        var dashboard = new EmployeeDashboardDto
        {
            AsOfDate = asOfDate,
            EmployeeId = employeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}"
        };

        // Get all data
        var timesheets = (await _timesheetRepository.GetByEmployeeIdAsync(employeeId, cancellationToken)).ToList();
        var expenseReports = (await _expenseReportRepository.GetByEmployeeIdAsync(employeeId, cancellationToken)).ToList();
        var projects = (await _projectRepository.GetAllAsync(cancellationToken)).ToList();

        // Calculate metrics
        CalculateTimeTrackingMetrics(dashboard, timesheets, currentWeekStart, currentMonthStart);
        CalculateTimesheetStatus(dashboard, timesheets);
        CalculateExpenseMetrics(dashboard, expenseReports, currentMonthStart);
        CalculateProjectAssignments(dashboard, timesheets, projects);
        GenerateChartData(dashboard, timesheets, currentWeekStart, currentMonthStart);

        // Get action items
        dashboard.PendingActions = GetPendingActions(timesheets, expenseReports);

        return dashboard;
    }

    private void CalculateTimeTrackingMetrics(
        EmployeeDashboardDto dashboard,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        DateTime weekStart,
        DateTime monthStart)
    {
        var weekTimesheets = timesheets
            .Where(t => t.PeriodStart >= weekStart && t.PeriodStart < weekStart.AddDays(7))
            .ToList();

        var monthTimesheets = timesheets
            .Where(t => t.PeriodStart >= monthStart && t.PeriodStart < monthStart.AddMonths(1))
            .ToList();

        var hoursThisWeek = weekTimesheets.SelectMany(t => t.Entries).Sum(e => e.Hours);
        var hoursThisMonth = monthTimesheets.SelectMany(t => t.Entries).Sum(e => e.Hours);
        var billableHoursMonth = monthTimesheets.SelectMany(t => t.Entries).Where(e => e.IsBillable).Sum(e => e.Hours);
        var nonBillableHoursMonth = hoursThisMonth - billableHoursMonth;
        var utilizationRate = hoursThisMonth > 0 ? (billableHoursMonth / hoursThisMonth) * 100 : 0;
        var overtimeHours = hoursThisMonth > 160 ? hoursThisMonth - 160 : 0; // Assuming 160 standard hours/month

        dashboard.HoursThisWeek = CreateKpiMetric(KpiType.BillableHours, "Hours This Week", hoursThisWeek, null, 40m, null, "hours");
        dashboard.HoursThisMonth = CreateKpiMetric(KpiType.BillableHours, "Hours This Month", hoursThisMonth, null, 160m, null, "hours");
        dashboard.BillableHoursThisMonth = CreateKpiMetric(KpiType.BillableHours, "Billable Hours", billableHoursMonth, null, null, null, "hours");
        dashboard.NonBillableHoursThisMonth = CreateKpiMetric(KpiType.NonBillableHours, "Non-Billable Hours", nonBillableHoursMonth, null, null, null, "hours");
        dashboard.UtilizationRate = CreateKpiMetric(KpiType.EmployeeUtilization, "Utilization Rate", utilizationRate, null, 75m, null, "percentage");
        dashboard.OvertimeHours = CreateKpiMetric(KpiType.BillableHours, "Overtime Hours", overtimeHours, null, null, null, "hours");
    }

    private void CalculateTimesheetStatus(EmployeeDashboardDto dashboard, List<ERP.Domain.TE.Entities.Timesheet> timesheets)
    {
        var pendingTimesheets = timesheets.Count(t => t.Status == TimesheetStatus.Draft);
        var rejectedTimesheets = timesheets.Count(t => t.Status == TimesheetStatus.Rejected);
        var overdueTimesheets = timesheets.Count(t => t.Status == TimesheetStatus.Draft && t.PeriodEnd < DateTime.UtcNow.AddDays(-7));

        dashboard.PendingTimesheets = CreateKpiMetric(KpiType.OverdueTimesheets, "Pending Timesheets", pendingTimesheets, null, null, null, "number");
        dashboard.RejectedTimesheets = CreateKpiMetric(KpiType.OverdueTimesheets, "Rejected Timesheets", rejectedTimesheets, null, null, null, "number");
        dashboard.OverdueTimesheets = CreateKpiMetric(KpiType.OverdueTimesheets, "Overdue Timesheets", overdueTimesheets, null, null, null, "number");
    }

    private void CalculateExpenseMetrics(
        EmployeeDashboardDto dashboard,
        List<ERP.Domain.TE.Entities.ExpenseReport> expenseReports,
        DateTime monthStart)
    {
        var monthExpenses = expenseReports
            .Where(e => e.CreatedDate >= monthStart && e.CreatedDate < monthStart.AddMonths(1))
            .ToList();

        var expensesThisMonth = monthExpenses.SelectMany(e => e.Expenses).Sum(ex => ex.Amount.Amount);
        var pendingExpenses = expenseReports.Count(e => e.Status == ExpenseReportStatus.Draft || e.Status == ExpenseReportStatus.Submitted);
        var approvedExpenses = expenseReports.Count(e => e.Status == ExpenseReportStatus.Approved);
        var rejectedExpenses = expenseReports.Count(e => e.Status == ExpenseReportStatus.Rejected);
        var reimbursementDue = expenseReports
            .Where(e => e.Status == ExpenseReportStatus.Approved)
            .SelectMany(e => e.Expenses)
            .Sum(ex => ex.Amount.Amount);

        dashboard.ExpensesThisMonth = CreateKpiMetric(KpiType.Revenue, "Expenses This Month", expensesThisMonth, null, null, "USD", "currency");
        dashboard.PendingExpenses = CreateKpiMetric(KpiType.Revenue, "Pending Expenses", pendingExpenses, null, null, null, "number");
        dashboard.ApprovedExpenses = CreateKpiMetric(KpiType.Revenue, "Approved Expenses", approvedExpenses, null, null, null, "number");
        dashboard.RejectedExpenses = CreateKpiMetric(KpiType.Revenue, "Rejected Expenses", rejectedExpenses, null, null, null, "number");
        dashboard.ReimbursementDue = CreateKpiMetric(KpiType.Revenue, "Reimbursement Due", reimbursementDue, null, null, "USD", "currency");
    }

    private void CalculateProjectAssignments(
        EmployeeDashboardDto dashboard,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        List<ERP.Domain.PM.Entities.Project> projects)
    {
        // Get unique projects from timesheets
        var projectIds = timesheets
            .SelectMany(t => t.Entries)
            .Select(e => e.ProjectId)
            .Distinct()
            .ToList();

        var assignedProjects = projects.Where(p => projectIds.Contains(p.Id)).ToList();
        var activeProjects = assignedProjects.Count(p => p.Status == ProjectStatus.Active);
        var completedProjects = assignedProjects.Count(p => p.Status == ProjectStatus.Completed);

        dashboard.ActiveProjects = CreateKpiMetric(KpiType.ActiveProjects, "Active Projects", activeProjects, null, null, null, "number");
        dashboard.ProjectsCompleted = CreateKpiMetric(KpiType.ActiveProjects, "Projects Completed", completedProjects, null, null, null, "number");

        // Create project assignments
        dashboard.CurrentAssignments = assignedProjects
            .Where(p => p.Status == ProjectStatus.Active)
            .Select(p =>
            {
                var projectTimesheets = timesheets
                    .SelectMany(t => t.Entries)
                    .Where(e => e.ProjectId == p.Id)
                    .ToList();

                var hoursUsed = projectTimesheets.Sum(e => e.Hours);

                return new ProjectAssignmentDto
                {
                    ProjectId = p.Id,
                    ProjectNumber = p.ProjectNumber,
                    ProjectName = p.Name,
                    ClientName = "Client", // Would join with client
                    Status = p.Status,
                    RoleName = "Team Member", // Would come from resource allocation
                    AllocatedHours = 160m, // Would come from resource allocation
                    HoursUsed = hoursUsed,
                    HoursRemaining = 160m - hoursUsed,
                    StartDate = p.CreatedDate,
                    EndDate = null
                };
            })
            .ToList();

        // Time off metrics (placeholder)
        dashboard.VacationDaysAvailable = CreateKpiMetric(KpiType.ActiveProjects, "Vacation Days Available", 15, null, null, null, "number");
        dashboard.SickDaysUsed = CreateKpiMetric(KpiType.ActiveProjects, "Sick Days Used", 2, null, null, null, "number");
        dashboard.TimeOffRequests = CreateKpiMetric(KpiType.ActiveProjects, "Time Off Requests", 0, null, null, null, "number");
    }

    private void GenerateChartData(
        EmployeeDashboardDto dashboard,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        DateTime weekStart,
        DateTime monthStart)
    {
        // Hours this week by day
        dashboard.HoursThisWeekByDay = new List<ChartDataPointDto>();
        for (int i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            var dayTimesheets = timesheets
                .Where(t => t.PeriodStart <= day && t.PeriodEnd >= day)
                .SelectMany(t => t.Entries)
                .ToList();

            var hours = dayTimesheets.Sum(e => e.Hours) / 5; // Approximate daily hours

            dashboard.HoursThisWeekByDay.Add(new ChartDataPointDto
            {
                Label = day.ToString("ddd"),
                Date = day,
                Value = hours
            });
        }

        // Hours by project (pie chart)
        var projectHours = timesheets
            .Where(t => t.PeriodStart >= monthStart)
            .SelectMany(t => t.Entries)
            .GroupBy(e => e.ProjectId)
            .Select(g => new PieChartDataDto
            {
                Label = $"Project {g.Key}",
                Value = g.Sum(e => e.Hours),
                Percentage = 0
            })
            .ToList();

        var totalHours = projectHours.Sum(p => p.Value);
        foreach (var item in projectHours)
        {
            item.Percentage = totalHours > 0 ? (item.Value / totalHours) * 100 : 0;
        }

        dashboard.HoursByProject = projectHours;

        // Utilization trend (placeholder)
        dashboard.UtilizationTrend = new List<ChartDataPointDto>();
    }

    private List<ActionItemDto> GetPendingActions(
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        List<ERP.Domain.TE.Entities.ExpenseReport> expenseReports)
    {
        var actions = new List<ActionItemDto>();

        // Pending timesheets
        var pendingTimesheets = timesheets.Where(t => t.Status == TimesheetStatus.Draft).ToList();
        foreach (var ts in pendingTimesheets)
        {
            actions.Add(new ActionItemDto
            {
                Type = "Timesheet",
                Description = $"Submit timesheet for {ts.PeriodStart:MM/dd} - {ts.PeriodEnd:MM/dd}",
                DueDate = ts.PeriodEnd.AddDays(7),
                IsOverdue = ts.PeriodEnd.AddDays(7) < DateTime.UtcNow,
                Priority = "High"
            });
        }

        // Rejected timesheets
        var rejectedTimesheets = timesheets.Where(t => t.Status == TimesheetStatus.Rejected).ToList();
        foreach (var ts in rejectedTimesheets)
        {
            actions.Add(new ActionItemDto
            {
                Type = "Timesheet",
                Description = $"Revise rejected timesheet for {ts.PeriodStart:MM/dd} - {ts.PeriodEnd:MM/dd}",
                DueDate = DateTime.UtcNow.AddDays(3),
                IsOverdue = false,
                Priority = "High"
            });
        }

        // Pending expense reports
        var pendingExpenses = expenseReports.Where(e => e.Status == ExpenseReportStatus.Draft).ToList();
        foreach (var exp in pendingExpenses)
        {
            actions.Add(new ActionItemDto
            {
                Type = "Expense",
                Description = $"Submit expense report: {exp.Title}",
                DueDate = DateTime.UtcNow.AddDays(14),
                IsOverdue = false,
                Priority = "Normal"
            });
        }

        return actions.OrderBy(a => a.DueDate).ToList();
    }

    private KpiMetricDto CreateKpiMetric(KpiType type, string name, decimal currentValue, decimal? previousValue, decimal? targetValue, string? currency, string format)
    {
        var percentageChange = previousValue.HasValue && previousValue.Value != 0
            ? ((currentValue - previousValue.Value) / previousValue.Value) * 100
            : (decimal?)null;

        var trend = TrendDirection.Flat;
        if (percentageChange.HasValue)
        {
            if (percentageChange.Value > 2) trend = TrendDirection.Up;
            else if (percentageChange.Value < -2) trend = TrendDirection.Down;
        }

        var isHealthy = true;
        if (targetValue.HasValue)
        {
            isHealthy = currentValue >= targetValue.Value;
        }

        return new KpiMetricDto
        {
            Type = type,
            Name = name,
            CurrentValue = Math.Round(currentValue, 2),
            PreviousValue = previousValue.HasValue ? Math.Round(previousValue.Value, 2) : null,
            TargetValue = targetValue.HasValue ? Math.Round(targetValue.Value, 2) : null,
            PercentageChange = percentageChange.HasValue ? Math.Round(percentageChange.Value, 2) : null,
            Trend = trend,
            Currency = currency,
            Format = format,
            IsHealthy = isHealthy
        };
    }
}
