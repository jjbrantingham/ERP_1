using ERP.Application.RPT.DTOs;
using ERP.Application.RPT.Queries;
using ERP.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.RPT.Handlers;

/// <summary>
/// Handler for Project Status report
/// NOTE: Simplified implementation - full implementation requires WBS tracking
/// </summary>
public class GetProjectStatusQueryHandler : IRequestHandler<GetProjectStatusQuery, List<ProjectStatusDto>>
{
    private readonly ERPDbContext _context;

    public GetProjectStatusQueryHandler(ERPDbContext context)
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
    private readonly ERPDbContext _context;
    private readonly Application.RPT.Services.IReportPeriodService _periodService;

    public GetResourceUtilizationQueryHandler(ERPDbContext context, Application.RPT.Services.IReportPeriodService periodService)
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
/// Handler for Executive Dashboard
/// NOTE: Simplified implementation
/// </summary>
public class GetExecutiveDashboardQueryHandler : IRequestHandler<GetExecutiveDashboardQuery, ExecutiveDashboardDto>
{
    private readonly ERPDbContext _context;

    public GetExecutiveDashboardQueryHandler(ERPDbContext context)
    {
        _context = context;
    }

    public async Task<ExecutiveDashboardDto> Handle(GetExecutiveDashboardQuery request, CancellationToken cancellationToken)
    {
        var asOfDate = request.AsOfDate ?? DateTime.UtcNow;

        var dashboard = new ExecutiveDashboardDto
        {
            AsOfDate = asOfDate,
            FinancialKPIs = new FinancialKPIsDto
            {
                TotalRevenue = 0, // TODO: Calculate from GL
                TotalExpenses = 0,
                NetIncome = 0,
                AccountsReceivable = await _context.Invoices
                    .Where(i => i.Status == ERP.Domain.BILL.Enums.InvoiceStatus.Posted)
                    .SumAsync(i => i.TotalAmount, cancellationToken),
                OutstandingInvoices = await _context.Invoices
                    .Where(i => i.Status == ERP.Domain.BILL.Enums.InvoiceStatus.Posted)
                    .CountAsync(cancellationToken)
            },
            ProjectKPIs = new ProjectKPIsDto
            {
                TotalProjects = await _context.Projects.CountAsync(cancellationToken),
                ActiveProjects = await _context.Projects
                    .Where(p => p.Status == ERP.Domain.PM.Enums.ProjectStatus.Active)
                    .CountAsync(cancellationToken)
            },
            OperationalKPIs = new OperationalKPIsDto
            {
                TotalEmployees = await _context.Employees.CountAsync(cancellationToken),
                ActiveEmployees = await _context.Employees
                    .Where(e => e.IsActive)
                    .CountAsync(cancellationToken),
                PendingTimesheets = await _context.Timesheets
                    .Where(t => t.Status == ERP.Domain.TE.Enums.TimesheetStatus.Pending)
                    .CountAsync(cancellationToken)
            }
        };

        return dashboard;
    }
}

/// <summary>
/// Handler for Timesheet Summary report
/// </summary>
public class GetTimesheetSummaryQueryHandler : IRequestHandler<GetTimesheetSummaryQuery, TimesheetSummaryDto>
{
    private readonly ERPDbContext _context;

    public GetTimesheetSummaryQueryHandler(ERPDbContext context)
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

// Placeholder handlers for other reports
public class GetProjectProfitabilityQueryHandler : IRequestHandler<GetProjectProfitabilityQuery, List<ProjectProfitabilityDto>>
{
    public Task<List<ProjectProfitabilityDto>> Handle(GetProjectProfitabilityQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement full profitability calculations
        return Task.FromResult(new List<ProjectProfitabilityDto>());
    }
}

public class GetBudgetVarianceQueryHandler : IRequestHandler<GetBudgetVarianceQuery, BudgetVarianceDto>
{
    public Task<BudgetVarianceDto> Handle(GetBudgetVarianceQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement budget variance calculations
        return Task.FromResult(new BudgetVarianceDto { ProjectId = request.ProjectId });
    }
}

public class GetExpenseSummaryQueryHandler : IRequestHandler<GetExpenseSummaryQuery, ExpenseSummaryDto>
{
    public Task<ExpenseSummaryDto> Handle(GetExpenseSummaryQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement expense summary
        return Task.FromResult(new ExpenseSummaryDto());
    }
}

public class GetProjectManagerDashboardQueryHandler : IRequestHandler<GetProjectManagerDashboardQuery, ProjectManagerDashboardDto>
{
    public Task<ProjectManagerDashboardDto> Handle(GetProjectManagerDashboardQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement PM dashboard
        return Task.FromResult(new ProjectManagerDashboardDto());
    }
}

public class GetFinanceDashboardQueryHandler : IRequestHandler<GetFinanceDashboardQuery, FinanceDashboardDto>
{
    public Task<FinanceDashboardDto> Handle(GetFinanceDashboardQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement finance dashboard
        return Task.FromResult(new FinanceDashboardDto());
    }
}

public class GetEmployeeDashboardQueryHandler : IRequestHandler<GetEmployeeDashboardQuery, EmployeeDashboardDto>
{
    public Task<EmployeeDashboardDto> Handle(GetEmployeeDashboardQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement employee dashboard
        return Task.FromResult(new EmployeeDashboardDto());
    }
}
