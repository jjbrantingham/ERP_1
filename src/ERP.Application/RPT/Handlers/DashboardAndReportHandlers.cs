using ERP.Application.RPT.DTOs;
using ERP.Application.Common.Interfaces;
using ERP.Application.RPT.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.RPT.Handlers;

/// <summary>
/// Handler for Project Status report
/// NOTE: Simplified implementation - full implementation requires WBS tracking
/// </summary>
public class GetProjectStatusQueryHandler : IRequestHandler<GetProjectStatusQuery, List<ProjectStatusDto>>
{
    private readonly IDbContext _context;

    public GetProjectStatusQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectStatusDto>> Handle(GetProjectStatusQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Projects.Include(p => p.Client).AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(p => p.Id == request.ProjectId.Value);
        if (request.ClientId.HasValue)
            query = query.Where(p => p.ClientId == request.ClientId.Value);
        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(p => p.Status.ToString() == request.Status);

        var projects = await query.ToListAsync(cancellationToken);

        return projects.Select(p => new ProjectStatusDto
        {
            ProjectId = p.Id,
            ProjectNumber = p.ProjectNumber,
            ProjectName = p.Name,
            ClientName = p.Client?.Name ?? "",
            Status = p.Status.ToString(),
            BudgetAmount = 0, // TODO: Calculate from WBS
            ActualCost = 0, // TODO: Calculate from timesheets + expenses
            PercentComplete = 0 // TODO: Calculate from WBS
        }).ToList();
    }
}

/// <summary>
/// Handler for Resource Utilization report
/// NOTE: Simplified implementation
/// </summary>
public class GetResourceUtilizationQueryHandler : IRequestHandler<GetResourceUtilizationQuery, ResourceUtilizationDto>
{
    private readonly IDbContext _context;
    private readonly Application.RPT.Services.IReportPeriodService _periodService;

    public GetResourceUtilizationQueryHandler(IDbContext context, Application.RPT.Services.IReportPeriodService periodService)
    {
        _context = context;
        _periodService = periodService;
    }

    public async Task<ResourceUtilizationDto> Handle(GetResourceUtilizationQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = _periodService.GetDateRange(request.Period, request.StartDate, request.EndDate);

        var timesheets = await _context.Timesheets
            .Include(t => t.Entries)
            .Include(t => t.Employee)
            .Where(t => t.WeekStartDate >= startDate && t.WeekEndDate <= endDate)
            .Where(t => t.Status == ERP.Domain.TE.Enums.TimesheetStatus.Approved)
            .ToListAsync(cancellationToken);

        var employeeUtilization = timesheets
            .GroupBy(t => new { t.EmployeeId, t.Employee.FirstName, t.Employee.LastName, t.Employee.Department, t.Employee.Title })
            .Select(g => new EmployeeUtilizationDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = $"{g.Key.FirstName} {g.Key.LastName}",
                Department = g.Key.Department ?? "",
                Title = g.Key.Title ?? "",
                TotalHours = g.SelectMany(t => t.Entries).Sum(e => e.Hours),
                BillableHours = g.SelectMany(t => t.Entries).Where(e => e.IsBillable).Sum(e => e.Hours),
                NonBillableHours = g.SelectMany(t => t.Entries).Where(e => !e.IsBillable).Sum(e => e.Hours),
                AvailableHours = 160, // Standard 40hrs/week * 4 weeks
                UtilizationRate = 0,
                BillableRate = 0,
                ProjectCount = g.SelectMany(t => t.Entries).Select(e => e.ProjectId).Distinct().Count()
            })
            .ToList();

        // Calculate utilization rates
        foreach (var emp in employeeUtilization)
        {
            emp.UtilizationRate = emp.AvailableHours > 0 ? (emp.TotalHours / emp.AvailableHours) * 100 : 0;
            emp.BillableRate = emp.TotalHours > 0 ? (emp.BillableHours / emp.TotalHours) * 100 : 0;
        }

        return new ResourceUtilizationDto
        {
            StartDate = startDate,
            EndDate = endDate,
            Employees = employeeUtilization,
            TotalBillableHours = employeeUtilization.Sum(e => e.BillableHours),
            TotalAvailableHours = employeeUtilization.Sum(e => e.AvailableHours),
            AverageUtilization = employeeUtilization.Average(e => e.UtilizationRate)
        };
    }
}

/// <summary>
/// Handler for Timesheet Summary report
/// </summary>
public class GetTimesheetSummaryQueryHandler : IRequestHandler<GetTimesheetSummaryQuery, TimesheetSummaryDto>
{
    private readonly IDbContext _context;

    public GetTimesheetSummaryQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<TimesheetSummaryDto> Handle(GetTimesheetSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Entries).ThenInclude(e => e.Project)
            .AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(t => t.WeekStartDate >= request.StartDate.Value);
        if (request.EndDate.HasValue)
            query = query.Where(t => t.WeekEndDate <= request.EndDate.Value);
        if (request.EmployeeId.HasValue)
            query = query.Where(t => t.EmployeeId == request.EmployeeId.Value);

        var timesheets = await query.ToListAsync(cancellationToken);

        var lines = timesheets.SelectMany(t => t.Entries.GroupBy(e => new { t.EmployeeId, t.Employee, e.ProjectId, e.Project })
            .Select(g => new TimesheetSummaryLineDto
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = $"{g.Key.Employee.FirstName} {g.Key.Employee.LastName}",
                ProjectId = g.Key.ProjectId,
                ProjectName = g.Key.Project?.Name ?? "No Project",
                TotalHours = g.Sum(e => e.Hours),
                BillableHours = g.Where(e => e.IsBillable).Sum(e => e.Hours),
                NonBillableHours = g.Where(e => !e.IsBillable).Sum(e => e.Hours),
                Status = g.First().Timesheet.Status.ToString()
            })).ToList();

        return new TimesheetSummaryDto
        {
            StartDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30),
            EndDate = request.EndDate ?? DateTime.UtcNow,
            Lines = lines,
            TotalHours = lines.Sum(l => l.TotalHours),
            BillableHours = lines.Sum(l => l.BillableHours),
            NonBillableHours = lines.Sum(l => l.NonBillableHours),
            EmployeeCount = lines.Select(l => l.EmployeeId).Distinct().Count(),
            ProjectCount = lines.Select(l => l.ProjectId).Distinct().Count()
        };
    }
}

// Full implementations for remaining reports
public class GetProjectProfitabilityQueryHandler : IRequestHandler<GetProjectProfitabilityQuery, List<ProjectProfitabilityDto>>
{
    private readonly IDbContext _context;
    private readonly Application.RPT.Services.IReportPeriodService _periodService;

    public GetProjectProfitabilityQueryHandler(IDbContext context, Application.RPT.Services.IReportPeriodService periodService)
    {
        _context = context;
        _periodService = periodService;
    }

    public async Task<List<ProjectProfitabilityDto>> Handle(GetProjectProfitabilityQuery request, CancellationToken cancellationToken)
    {
        var (startDate, endDate) = _periodService.GetDateRange(request.Period, request.StartDate, request.EndDate);

        var query = _context.Projects
            .Include(p => p.Client)
            .Include(p => p.Invoices)
            .AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(p => p.Id == request.ProjectId.Value);

        var projects = await query.ToListAsync(cancellationToken);
        var results = new List<ProjectProfitabilityDto>();

        foreach (var project in projects)
        {
            // Calculate revenue from invoices
            var invoices = await _context.Invoices
                .Where(i => i.ProjectId == project.Id)
                .Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
                .Where(i => i.Status == ERP.Domain.BILL.Enums.InvoiceStatus.Posted ||
                           i.Status == ERP.Domain.BILL.Enums.InvoiceStatus.PartiallyPaid ||
                           i.Status == ERP.Domain.BILL.Enums.InvoiceStatus.Paid)
                .ToListAsync(cancellationToken);

            var totalRevenue = invoices.Sum(i => i.TotalAmount);

            // Calculate labor cost from timesheets
            var timesheets = await _context.Timesheets
                .Include(t => t.Entries).ThenInclude(e => e.Employee)
                .Where(t => t.Entries.Any(e => e.ProjectId == project.Id))
                .Where(t => t.WeekStartDate >= startDate && t.WeekEndDate <= endDate)
                .Where(t => t.Status == ERP.Domain.TE.Enums.TimesheetStatus.Approved)
                .ToListAsync(cancellationToken);

            var projectEntries = timesheets.SelectMany(t => t.Entries)
                .Where(e => e.ProjectId == project.Id)
                .ToList();

            var billableHours = projectEntries.Where(e => e.IsBillable).Sum(e => e.Hours);

            // Calculate labor cost from timesheets
            var laborCost = 0m;
            if (projectEntries.Any())
            {
                // Get all employee rates for the date range in a single query to avoid N+1
                var employeeIds = projectEntries.Select(e => e.EmployeeId).Distinct().ToList();
                var minDate = projectEntries.Min(e => e.WorkDate);
                var maxDate = projectEntries.Max(e => e.WorkDate);

                var allRates = await _context.Rates
                    .Where(r => employeeIds.Contains(r.EmployeeId))
                    .Where(r => r.EffectiveDate <= maxDate)
                    .ToListAsync(cancellationToken);

                var ratesByEmployee = allRates
                    .GroupBy(r => r.EmployeeId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.EffectiveDate).ToList());

                // Calculate labor cost using cached rates
                foreach (var entry in projectEntries)
                {
                    if (ratesByEmployee.TryGetValue(entry.EmployeeId, out var employeeRates))
                    {
                        var rate = employeeRates.FirstOrDefault(r => r.EffectiveDate <= entry.WorkDate);
                        laborCost += (rate?.CostRate ?? 0) * entry.Hours;
                    }
                }
            }

            // Calculate expense cost
            var expenses = await _context.ExpenseReports
                .Include(e => e.Items)
                .Where(e => e.ProjectId == project.Id)
                .Where(e => e.ReportDate >= startDate && e.ReportDate <= endDate)
                .Where(e => e.Status == ERP.Domain.TE.Enums.ExpenseReportStatus.Approved)
                .ToListAsync(cancellationToken);

            var expenseCost = expenses.Sum(e => e.TotalAmount);

            // Calculate billable revenue (from billable hours)
            var billableRevenue = invoices.Where(i => i.BillingMode == ERP.Domain.BILL.Enums.BillingMode.TimeAndMaterial)
                .Sum(i => i.TotalAmount);

            var totalCost = laborCost + expenseCost;
            var grossProfit = totalRevenue - totalCost;
            var grossProfitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0;
            var realizationRate = laborCost > 0 ? (billableRevenue / laborCost) * 100 : 0;

            results.Add(new ProjectProfitabilityDto
            {
                ProjectId = project.Id,
                ProjectNumber = project.ProjectNumber,
                ProjectName = project.Name,
                ClientName = project.Client?.Name ?? "",
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                GrossProfit = grossProfit,
                GrossProfitMargin = grossProfitMargin,
                BillableHours = billableHours,
                BillableRevenue = billableRevenue,
                ExpenseRevenue = invoices.Sum(i => i.TotalAmount) - billableRevenue,
                LaborCost = laborCost,
                ExpenseCost = expenseCost,
                OverheadCost = 0, // TODO: Calculate overhead allocation
                RealizationRate = realizationRate
            });
        }

        return results;
    }
}

public class GetBudgetVarianceQueryHandler : IRequestHandler<GetBudgetVarianceQuery, BudgetVarianceDto>
{
    private readonly IDbContext _context;

    public GetBudgetVarianceQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<BudgetVarianceDto> Handle(GetBudgetVarianceQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.WBSItems)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
            return new BudgetVarianceDto { ProjectId = request.ProjectId };

        // Get actual costs from timesheets and expenses
        var timesheets = await _context.Timesheets
            .Include(t => t.Entries).ThenInclude(e => e.Employee)
            .Where(t => t.Entries.Any(e => e.ProjectId == request.ProjectId))
            .Where(t => t.Status == ERP.Domain.TE.Enums.TimesheetStatus.Approved)
            .ToListAsync(cancellationToken);

        var expenses = await _context.ExpenseReports
            .Include(e => e.Items)
            .Where(e => e.ProjectId == request.ProjectId)
            .Where(e => e.Status == ERP.Domain.TE.Enums.ExpenseReportStatus.Approved)
            .ToListAsync(cancellationToken);

        var lines = new List<BudgetVarianceLineDto>();

        // Get all timesheet entries for this project to avoid N+1
        var allProjectEntries = timesheets.SelectMany(t => t.Entries).ToList();

        // Get all employee rates for the date range in a single query to avoid N+1
        var employeeIds = allProjectEntries.Select(e => e.EmployeeId).Distinct().ToList();
        if (employeeIds.Any())
        {
            var minDate = allProjectEntries.Min(e => e.WorkDate);
            var maxDate = allProjectEntries.Max(e => e.WorkDate);

            var allRates = await _context.Rates
                .Where(r => employeeIds.Contains(r.EmployeeId))
                .Where(r => r.EffectiveDate <= maxDate)
                .ToListAsync(cancellationToken);

            var ratesByEmployee = allRates
                .GroupBy(r => r.EmployeeId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.EffectiveDate).ToList());

            // Calculate variance by WBS item
            foreach (var wbs in project.WBSItems)
            {
                var budgetedAmount = wbs.BudgetedAmount;

                // Get actual for this WBS
                var wbsEntries = allProjectEntries
                    .Where(e => e.WBSItemId == wbs.Id)
                    .ToList();

                var actualAmount = 0m;
                foreach (var entry in wbsEntries)
                {
                    if (ratesByEmployee.TryGetValue(entry.EmployeeId, out var employeeRates))
                    {
                        var rate = employeeRates.FirstOrDefault(r => r.EffectiveDate <= entry.WorkDate);
                        actualAmount += (rate?.CostRate ?? 0) * entry.Hours;
                    }
                }

                var variance = budgetedAmount - actualAmount;
                var variancePercentage = budgetedAmount > 0 ? (variance / budgetedAmount) * 100 : 0;

                lines.Add(new BudgetVarianceLineDto
                {
                    Category = "Labor",
                    WBSItem = wbs.Name,
                    BudgetedAmount = budgetedAmount,
                    ActualAmount = actualAmount,
                    Variance = variance,
                    VariancePercentage = variancePercentage,
                    Status = variance >= 0 ? (variance > budgetedAmount * 0.1m ? "Under" : "OnTrack") : "Over"
                });
            }
        }

        return new BudgetVarianceDto
        {
            ProjectId = request.ProjectId,
            ProjectNumber = project.ProjectNumber,
            ProjectName = project.Name,
            Lines = lines,
            TotalBudget = lines.Sum(l => l.BudgetedAmount),
            TotalActual = lines.Sum(l => l.ActualAmount),
            TotalVariance = lines.Sum(l => l.Variance),
            VariancePercentage = lines.Sum(l => l.BudgetedAmount) > 0
                ? (lines.Sum(l => l.Variance) / lines.Sum(l => l.BudgetedAmount)) * 100
                : 0
        };
    }
}

public class GetExpenseSummaryQueryHandler : IRequestHandler<GetExpenseSummaryQuery, ExpenseSummaryDto>
{
    private readonly IDbContext _context;

    public GetExpenseSummaryQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseSummaryDto> Handle(GetExpenseSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ExpenseReports
            .Include(e => e.Employee)
            .Include(e => e.Items)
            .AsQueryable();

        if (request.StartDate.HasValue)
            query = query.Where(e => e.ReportDate >= request.StartDate.Value);
        if (request.EndDate.HasValue)
            query = query.Where(e => e.ReportDate <= request.EndDate.Value);
        if (request.EmployeeId.HasValue)
            query = query.Where(e => e.EmployeeId == request.EmployeeId.Value);
        if (request.ProjectId.HasValue)
            query = query.Where(e => e.ProjectId == request.ProjectId.Value);

        var expenseReports = await query.ToListAsync(cancellationToken);

        var lines = expenseReports.SelectMany(e => e.Items.GroupBy(i => new { i.Category })
            .Select(g => new ExpenseSummaryLineDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeName = $"{e.Employee.FirstName} {e.Employee.LastName}",
                ProjectId = e.ProjectId,
                ProjectName = e.Project?.Name ?? "No Project",
                Category = g.Key.Category,
                TotalAmount = g.Sum(i => i.Amount),
                Status = e.Status.ToString(),
                ItemCount = g.Count()
            })).ToList();

        return new ExpenseSummaryDto
        {
            StartDate = request.StartDate ?? DateTime.UtcNow.AddDays(-30),
            EndDate = request.EndDate ?? DateTime.UtcNow,
            Lines = lines,
            TotalExpenses = lines.Sum(l => l.TotalAmount),
            ApprovedExpenses = lines.Where(l => l.Status == "Approved").Sum(l => l.TotalAmount),
            PendingExpenses = lines.Where(l => l.Status == "Pending").Sum(l => l.TotalAmount),
            RejectedExpenses = lines.Where(l => l.Status == "Rejected").Sum(l => l.TotalAmount),
            EmployeeCount = lines.Select(l => l.EmployeeId).Distinct().Count(),
            ProjectCount = lines.Where(l => l.ProjectId.HasValue).Select(l => l.ProjectId).Distinct().Count()
        };
    }
}

public class GetProjectManagerDashboardQueryHandler : IRequestHandler<GetProjectManagerDashboardQuery, ProjectManagerDashboardDto>
{
    private readonly IDbContext _context;

    public GetProjectManagerDashboardQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectManagerDashboardDto> Handle(GetProjectManagerDashboardQuery request, CancellationToken cancellationToken)
    {
        var asOfDate = request.AsOfDate ?? DateTime.UtcNow;

        // Get projects managed by this user (simplified - assumes ProjectManagerId field exists or uses a different approach)
        var myProjects = await _context.Projects
            .Include(p => p.Client)
            .Where(p => p.Status == ERP.Domain.PM.Enums.ProjectStatus.Active)
            .Take(10) // Top 10 active projects
            .Select(p => new ProjectStatusDto
            {
                ProjectId = p.Id,
                ProjectNumber = p.ProjectNumber,
                ProjectName = p.Name,
                ClientName = p.Client != null ? p.Client.Name : "",
                Status = p.Status.ToString(),
                BudgetAmount = 0, // Simplified
                ActualCost = 0,
                PercentComplete = 0
            })
            .ToListAsync(cancellationToken);

        // Get pending approvals
        var pendingTimesheets = await _context.Timesheets
            .Where(t => t.Status == ERP.Domain.TE.Enums.TimesheetStatus.Submitted)
            .CountAsync(cancellationToken);

        var pendingExpenses = await _context.ExpenseReports
            .Where(e => e.Status == ERP.Domain.TE.Enums.ExpenseReportStatus.Submitted)
            .CountAsync(cancellationToken);

        return new ProjectManagerDashboardDto
        {
            AsOfDate = asOfDate,
            MyProjects = myProjects,
            UpcomingTasks = new List<TaskSummaryDto>(),
            UpcomingMilestones = new List<MilestoneSummaryDto>(),
            TeamUtilization = new ResourceUtilizationDto(),
            TeamBudgetUtilization = 0,
            PendingApprovals = pendingTimesheets + pendingExpenses
        };
    }
}

public class GetFinanceDashboardQueryHandler : IRequestHandler<GetFinanceDashboardQuery, FinanceDashboardDto>
{
    private readonly IDbContext _context;
    private readonly IMediator _mediator;

    public GetFinanceDashboardQueryHandler(IDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<FinanceDashboardDto> Handle(GetFinanceDashboardQuery request, CancellationToken cancellationToken)
    {
        var asOfDate = request.AsOfDate ?? DateTime.UtcNow;

        // Get AR Aging
        var arAging = await _mediator.Send(new GetARAgingQuery { AsOfDate = asOfDate }, cancellationToken);

        // Get recent invoices
        var recentInvoices = await _context.Invoices
            .Include(i => i.Client)
            .OrderByDescending(i => i.InvoiceDate)
            .Take(10)
            .Select(i => new InvoiceAgingLineDto
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                ClientId = i.ClientId,
                ClientName = i.Client != null ? i.Client.Name : "",
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                OutstandingAmount = i.TotalAmount,
                Status = i.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Get recent payments
        var recentPayments = await _context.Payments
            .Include(p => p.Invoice).ThenInclude(i => i.Client)
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .Select(p => new PaymentSummaryDto
            {
                PaymentId = p.Id,
                PaymentDate = p.PaymentDate,
                ClientName = p.Invoice.Client != null ? p.Invoice.Client.Name : "",
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                ReferenceNumber = p.ReferenceNumber ?? ""
            })
            .ToListAsync(cancellationToken);

        return new FinanceDashboardDto
        {
            AsOfDate = asOfDate,
            KPIs = new FinancialKPIsDto
            {
                AccountsReceivable = arAging.TotalAR,
                OutstandingInvoices = arAging.Lines.Count
            },
            ARAgingSummary = arAging,
            RecentInvoices = recentInvoices,
            RecentPayments = recentPayments,
            CashFlowTrend = new List<TrendDataDto>(),
            ProjectedRevenue = 0,
            ProjectedExpenses = 0
        };
    }
}

public class GetEmployeeDashboardQueryHandler : IRequestHandler<GetEmployeeDashboardQuery, EmployeeDashboardDto>
{
    private readonly IDbContext _context;

    public GetEmployeeDashboardQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDashboardDto> Handle(GetEmployeeDashboardQuery request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return new EmployeeDashboardDto();

        // Get recent timesheets
        var recentTimesheets = await _context.Timesheets
            .Where(t => t.EmployeeId == request.EmployeeId)
            .OrderByDescending(t => t.WeekStartDate)
            .Take(5)
            .Select(t => new TimesheetStatusDto
            {
                TimesheetId = t.Id,
                WeekStartDate = t.WeekStartDate,
                WeekEndDate = t.WeekEndDate,
                TotalHours = t.TotalHours,
                Status = t.Status.ToString(),
                SubmittedDate = t.SubmittedDate,
                ApprovedDate = t.ApprovedDate
            })
            .ToListAsync(cancellationToken);

        // Get recent expense reports
        var recentExpenses = await _context.ExpenseReports
            .Where(e => e.EmployeeId == request.EmployeeId)
            .OrderByDescending(e => e.ReportDate)
            .Take(5)
            .Select(e => new ExpenseReportStatusDto
            {
                ExpenseReportId = e.Id,
                ReportNumber = e.ReportNumber,
                ReportDate = e.ReportDate,
                TotalAmount = e.TotalAmount,
                Status = e.Status.ToString(),
                ItemCount = e.Items.Count
            })
            .ToListAsync(cancellationToken);

        // Get current assignments (simplified - would need project assignment tracking)
        var currentAssignments = new List<ProjectAssignmentDto>();

        // Calculate current period hours
        var currentPeriodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var currentPeriodTimesheets = await _context.Timesheets
            .Include(t => t.Entries)
            .Where(t => t.EmployeeId == request.EmployeeId)
            .Where(t => t.WeekStartDate >= currentPeriodStart)
            .ToListAsync(cancellationToken);

        var currentPeriodHours = currentPeriodTimesheets.SelectMany(t => t.Entries).Sum(e => e.Hours);
        var currentPeriodBillableHours = currentPeriodTimesheets.SelectMany(t => t.Entries).Where(e => e.IsBillable).Sum(e => e.Hours);
        var utilizationRate = currentPeriodHours > 0 ? (currentPeriodBillableHours / currentPeriodHours) * 100 : 0;

        return new EmployeeDashboardDto
        {
            AsOfDate = request.AsOfDate ?? DateTime.UtcNow,
            EmployeeId = request.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            RecentTimesheets = recentTimesheets,
            RecentExpenseReports = recentExpenses,
            CurrentAssignments = currentAssignments,
            CurrentPeriodHours = currentPeriodHours,
            CurrentPeriodBillableHours = currentPeriodBillableHours,
            UtilizationRate = utilizationRate,
            PendingApprovals = recentTimesheets.Count(t => t.Status == "Pending") + recentExpenses.Count(e => e.Status == "Pending")
        };
    }
}
