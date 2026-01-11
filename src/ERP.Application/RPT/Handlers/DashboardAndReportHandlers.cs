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
        var query = _context.Projects.AsQueryable();

        if (request.ProjectId.HasValue)
            query = query.Where(p => p.Id == request.ProjectId.Value);
        if (request.ClientId.HasValue)
            query = query.Where(p => p.ClientId == request.ClientId.Value);
        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(p => p.Status.ToString() == request.Status);

        // Join with Clients to get client names
        var projectsWithClients = await query
            .GroupJoin(_context.Clients,
                p => p.ClientId,
                c => c.Id,
                (p, clients) => new { Project = p, Client = clients.FirstOrDefault() })
            .ToListAsync(cancellationToken);

        return projectsWithClients.Select(pc => new ProjectStatusDto
        {
            ProjectId = pc.Project.Id,
            ProjectNumber = pc.Project.ProjectNumber.Value,
            ProjectName = pc.Project.Name,
            ClientName = pc.Client?.Name ?? "",
            Status = pc.Project.Status.ToString(),
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
            .Where(t => t.PeriodStart >= startDate && t.PeriodEnd <= endDate)
            .Where(t => t.Status == ERP.Domain.TE.Enums.TimesheetStatus.Approved)
            .ToListAsync(cancellationToken);

        // Get employee IDs to fetch employee details
        var employeeIds = timesheets.Select(t => t.EmployeeId).Distinct().ToList();
        var employees = await _context.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        var employeeDict = employees.ToDictionary(e => e.Id);

        var employeeUtilization = timesheets
            .GroupBy(t => t.EmployeeId)
            .Select(g =>
            {
                var employee = employeeDict.GetValueOrDefault(g.Key);
                return new EmployeeUtilizationDto
                {
                    EmployeeId = g.Key,
                    EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                    Department = employee?.Department ?? "",
                    Title = employee?.JobTitle ?? "",
                    TotalHours = g.SelectMany(t => t.Entries).Sum(e => e.Hours),
                    BillableHours = g.SelectMany(t => t.Entries).Where(e => e.IsBillable).Sum(e => e.Hours),
                    NonBillableHours = g.SelectMany(t => t.Entries).Where(e => !e.IsBillable).Sum(e => e.Hours),
                    AvailableHours = 160, // Standard 40hrs/week * 4 weeks
                    UtilizationRate = 0,
                    BillableRate = 0,
                    ProjectCount = g.SelectMany(t => t.Entries).Select(e => e.ProjectId).Distinct().Count()
                };
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
            .Include(t => t.Entries)
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
        var allProjectEntries = timesheets
            .SelectMany(t => t.Entries, (t, e) => new { Timesheet = t, Entry = e })
            .ToList();

        // Get all employee rates for the date range in a single query to avoid N+1
        var employeeIds = allProjectEntries.Select(e => e.Timesheet.EmployeeId).Distinct().ToList();
        if (employeeIds.Any())
        {
            var minDate = allProjectEntries.Min(e => e.Entry.WorkDate);
            var maxDate = allProjectEntries.Max(e => e.Entry.WorkDate);

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
                var budgetedAmount = wbs.Budget?.Amount ?? 0;

                // Get actual for this WBS
                var wbsEntries = allProjectEntries
                    .Where(e => e.Entry.WBSItemId == wbs.Id)
                    .ToList();

                var actualAmount = 0m;
                foreach (var entry in wbsEntries)
                {
                    if (ratesByEmployee.TryGetValue(entry.Timesheet.EmployeeId, out var employeeRates))
                    {
                        var rate = employeeRates.FirstOrDefault(r => r.EffectiveDate <= entry.Entry.WorkDate);
                        actualAmount += (rate?.CostRate ?? 0) * entry.Entry.Hours;
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
            ProjectNumber = project.ProjectNumber.Value,
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
            .Where(p => p.Status == ERP.Domain.PM.Enums.ProjectStatus.Active)
            .Take(10) // Top 10 active projects
            .GroupJoin(_context.Clients,
                p => p.ClientId,
                c => c.Id,
                (p, clients) => new { Project = p, Client = clients.FirstOrDefault() })
            .Select(pc => new ProjectStatusDto
            {
                ProjectId = pc.Project.Id,
                ProjectNumber = pc.Project.ProjectNumber.Value,
                ProjectName = pc.Project.Name,
                ClientName = pc.Client != null ? pc.Client.Name : "",
                Status = pc.Project.Status.ToString(),
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
            .OrderByDescending(i => i.InvoiceDate)
            .Take(10)
            .GroupJoin(_context.Clients,
                i => i.ClientId,
                c => c.Id,
                (i, clients) => new { Invoice = i, Client = clients.FirstOrDefault() })
            .Select(ic => new InvoiceAgingLineDto
            {
                InvoiceId = ic.Invoice.Id,
                InvoiceNumber = ic.Invoice.InvoiceNumber.Value,
                ClientId = ic.Invoice.ClientId,
                ClientName = ic.Client != null ? ic.Client.Name : "",
                InvoiceDate = ic.Invoice.InvoiceDate,
                DueDate = ic.Invoice.DueDate,
                TotalAmount = ic.Invoice.TotalAmount,
                OutstandingAmount = ic.Invoice.TotalAmount,
                Status = ic.Invoice.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Get recent payments
        var recentPayments = await _context.Payments
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .GroupJoin(_context.Invoices,
                p => p.InvoiceId,
                i => (long?)i.Id,
                (p, invoices) => new { Payment = p, Invoice = invoices.FirstOrDefault() })
            .GroupJoin(_context.Clients,
                pi => pi.Invoice != null ? pi.Invoice.ClientId : 0,
                c => c.Id,
                (pi, clients) => new { pi.Payment, pi.Invoice, Client = clients.FirstOrDefault() })
            .Select(pic => new PaymentSummaryDto
            {
                PaymentId = pic.Payment.Id,
                PaymentDate = pic.Payment.PaymentDate,
                ClientName = pic.Client != null ? pic.Client.Name : "",
                Amount = pic.Payment.Amount,
                PaymentMethod = pic.Payment.PaymentMethod.ToString(),
                ReferenceNumber = pic.Payment.ReferenceNumber ?? ""
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
            .OrderByDescending(t => t.PeriodStart)
            .Take(5)
            .Select(t => new TimesheetStatusDto
            {
                TimesheetId = t.Id,
                WeekStartDate = t.PeriodStart,
                WeekEndDate = t.PeriodEnd,
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
            .Where(t => t.PeriodStart >= currentPeriodStart)
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
