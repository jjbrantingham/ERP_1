using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.DASH.DTOs;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using ERP.Domain.DASH.Enums;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using ERP.Domain.TE.Enums;
using ERP.Domain.TE.Repositories;
using MediatR;

namespace ERP.Application.DASH.Queries;

/// <summary>
/// Handler for getting project dashboard data
/// </summary>
public class GetProjectDashboardQueryHandler : IRequestHandler<GetProjectDashboardQuery, ProjectDashboardDto>
{
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectRepository _projectRepository;
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public GetProjectDashboardQueryHandler(
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser,
        IProjectRepository projectRepository,
        ITimesheetRepository timesheetRepository,
        IInvoiceRepository invoiceRepository)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _projectRepository = projectRepository;
        _timesheetRepository = timesheetRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<ProjectDashboardDto> Handle(GetProjectDashboardQuery request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var asOfDate = request.AsOfDate ?? DateTime.UtcNow;
        var currency = request.Currency;

        var dashboard = new ProjectDashboardDto
        {
            AsOfDate = asOfDate,
            TenantId = _currentTenant.TenantId
        };

        // Get all data
        var projects = (await _projectRepository.GetAllAsync(cancellationToken)).ToList();
        var timesheets = (await _timesheetRepository.GetAllAsync(cancellationToken)).ToList();
        var invoices = (await _invoiceRepository.GetAllAsync(cancellationToken)).ToList();

        // Calculate metrics
        CalculateProjectPortfolioMetrics(dashboard, projects);
        CalculateBudgetMetrics(dashboard, projects, timesheets, invoices, currency);
        CalculateTimeMetrics(dashboard, projects, timesheets);
        CalculateProfitabilityMetrics(dashboard, projects, timesheets, invoices, currency);
        CalculateRiskMetrics(dashboard, projects, timesheets);

        // Get project details
        dashboard.AllProjects = GetProjectPerformances(projects, timesheets, invoices);
        dashboard.AtRiskProjects = dashboard.AllProjects.Where(p => p.IsAtRisk).ToList();
        dashboard.MostProfitableProjects = dashboard.AllProjects
            .OrderByDescending(p => p.ProfitMargin)
            .Take(5)
            .ToList();

        // Generate chart data
        GenerateChartData(dashboard, projects);

        return dashboard;
    }

    private void CalculateProjectPortfolioMetrics(ProjectDashboardDto dashboard, List<ERP.Domain.PM.Entities.Project> projects)
    {
        var totalProjects = projects.Count;
        var activeProjects = projects.Count(p => p.Status == ProjectStatus.Active);
        var onHoldProjects = projects.Count(p => p.Status == ProjectStatus.OnHold);
        var completedThisMonth = projects.Count(p => p.Status == ProjectStatus.Completed &&
            p.ModifiedDate >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1));

        dashboard.TotalProjects = CreateKpiMetric(KpiType.ActiveProjects, "Total Projects", totalProjects, null, null, null, "number");
        dashboard.ActiveProjects = CreateKpiMetric(KpiType.ActiveProjects, "Active Projects", activeProjects, null, null, null, "number");
        dashboard.OnHoldProjects = CreateKpiMetric(KpiType.ActiveProjects, "On Hold Projects", onHoldProjects, null, null, null, "number");
        dashboard.CompletedProjectsThisMonth = CreateKpiMetric(KpiType.ActiveProjects, "Completed This Month", completedThisMonth, null, null, null, "number");
    }

    private void CalculateBudgetMetrics(
        ProjectDashboardDto dashboard,
        List<ERP.Domain.PM.Entities.Project> projects,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        List<ERP.Domain.BILL.Entities.Invoice> invoices,
        string currency)
    {
        var activeProjects = projects.Where(p => p.Status == ProjectStatus.Active).ToList();
        var totalBudget = activeProjects.Sum(p => p.Budget?.Amount ?? 0);

        var totalActualCost = 0m;
        var onBudgetCount = 0;
        var overBudgetCount = 0;
        var totalVariance = 0m;

        foreach (var project in activeProjects)
        {
            var projectTimesheets = timesheets
                .Where(t => t.Status == TimesheetStatus.Approved)
                .SelectMany(t => t.Entries)
                .Where(e => e.ProjectId == project.Id);

            var actualCost = projectTimesheets.Sum(e => e.Hours) * 100m; // Simplified
            totalActualCost += actualCost;

            var budget = project.Budget?.Amount ?? 0;
            if (actualCost <= budget)
                onBudgetCount++;
            else
                overBudgetCount++;

            totalVariance += (budget - actualCost);
        }

        var budgetRemaining = totalBudget - totalActualCost;
        var avgVariance = activeProjects.Count > 0 ? totalVariance / activeProjects.Count : 0;

        dashboard.TotalBudget = CreateKpiMetric(KpiType.ActiveProjects, "Total Budget", totalBudget, null, null, currency, "currency");
        dashboard.BudgetUtilized = CreateKpiMetric(KpiType.ActiveProjects, "Budget Utilized", totalActualCost, null, null, currency, "currency");
        dashboard.BudgetRemaining = CreateKpiMetric(KpiType.ActiveProjects, "Budget Remaining", budgetRemaining, null, null, currency, "currency");
        dashboard.ProjectsOnBudget = CreateKpiMetric(KpiType.ActiveProjects, "Projects On Budget", onBudgetCount, null, null, null, "number");
        dashboard.ProjectsOverBudget = CreateKpiMetric(KpiType.OverBudgetProjects, "Projects Over Budget", overBudgetCount, null, null, null, "number");
        dashboard.AverageBudgetVariance = CreateKpiMetric(KpiType.ActiveProjects, "Avg Budget Variance", avgVariance, null, null, currency, "currency");
    }

    private void CalculateTimeMetrics(
        ProjectDashboardDto dashboard,
        List<ERP.Domain.PM.Entities.Project> projects,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets)
    {
        var activeProjects = projects.Where(p => p.Status == ProjectStatus.Active).ToList();

        var totalPlannedHours = activeProjects.Sum(p => p.WBSItems.Sum(w => w.EstimatedHours ?? 0));
        var totalActualHours = timesheets
            .Where(t => t.Status == TimesheetStatus.Approved)
            .SelectMany(t => t.Entries)
            .Where(e => activeProjects.Any(p => p.Id == e.ProjectId))
            .Sum(e => e.Hours);

        var hoursVariance = totalPlannedHours - totalActualHours;

        // Resource utilization and capacity (simplified)
        var resourceUtilization = 75m; // Would calculate from actual resource data
        var availableCapacity = 25m;

        dashboard.TotalPlannedHours = CreateKpiMetric(KpiType.ActiveProjects, "Total Planned Hours", totalPlannedHours, null, null, null, "hours");
        dashboard.TotalActualHours = CreateKpiMetric(KpiType.ActiveProjects, "Total Actual Hours", totalActualHours, null, null, null, "hours");
        dashboard.HoursVariance = CreateKpiMetric(KpiType.ActiveProjects, "Hours Variance", hoursVariance, null, null, null, "hours");
        dashboard.ResourceUtilization = CreateKpiMetric(KpiType.ProjectUtilization, "Resource Utilization", resourceUtilization, null, 75m, null, "percentage");
        dashboard.AvailableCapacity = CreateKpiMetric(KpiType.Capacity, "Available Capacity", availableCapacity, null, null, null, "percentage");
    }

    private void CalculateProfitabilityMetrics(
        ProjectDashboardDto dashboard,
        List<ERP.Domain.PM.Entities.Project> projects,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        List<ERP.Domain.BILL.Entities.Invoice> invoices,
        string currency)
    {
        var activeProjects = projects.Where(p => p.Status == ProjectStatus.Active).ToList();

        var totalRevenue = 0m;
        var totalCost = 0m;
        var totalProfit = 0m;

        foreach (var project in activeProjects)
        {
            var projectTimesheets = timesheets
                .Where(t => t.Status == TimesheetStatus.Approved)
                .SelectMany(t => t.Entries)
                .Where(e => e.ProjectId == project.Id);

            var cost = projectTimesheets.Sum(e => e.Hours) * 100m;
            totalCost += cost;

            var projectInvoices = invoices.Where(i => i.ProjectId == project.Id &&
                (i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.Paid));
            var revenue = projectInvoices.Sum(i => i.CalculateTotal().Amount);
            totalRevenue += revenue;
        }

        totalProfit = totalRevenue - totalCost;
        var avgMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;

        dashboard.TotalRevenue = CreateKpiMetric(KpiType.Revenue, "Total Revenue", totalRevenue, null, null, currency, "currency");
        dashboard.TotalCost = CreateKpiMetric(KpiType.ActiveProjects, "Total Cost", totalCost, null, null, currency, "currency");
        dashboard.TotalProfit = CreateKpiMetric(KpiType.Profit, "Total Profit", totalProfit, null, null, currency, "currency");
        dashboard.AverageProfitMargin = CreateKpiMetric(KpiType.ProjectProfitability, "Avg Profit Margin", avgMargin, null, 30m, null, "percentage");
    }

    private void CalculateRiskMetrics(
        ProjectDashboardDto dashboard,
        List<ERP.Domain.PM.Entities.Project> projects,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets)
    {
        var activeProjects = projects.Where(p => p.Status == ProjectStatus.Active).ToList();

        // Projects at risk (simplified - over budget or behind schedule)
        var atRiskCount = 0; // Would calculate based on budget/schedule variance

        // Overdue deliverables (placeholder)
        var overdueDeliverables = 0;

        // Unapproved timesheets
        var unapprovedTimesheets = timesheets.Count(t => t.Status == TimesheetStatus.Submitted);

        dashboard.ProjectsAtRisk = CreateKpiMetric(KpiType.ActiveProjects, "Projects At Risk", atRiskCount, null, null, null, "number");
        dashboard.OverdueDeliverables = CreateKpiMetric(KpiType.ActiveProjects, "Overdue Deliverables", overdueDeliverables, null, null, null, "number");
        dashboard.UnapprovedTimesheets = CreateKpiMetric(KpiType.OverdueTimesheets, "Unapproved Timesheets", unapprovedTimesheets, null, null, null, "number");
    }

    private List<ProjectPerformanceDto> GetProjectPerformances(
        List<ERP.Domain.PM.Entities.Project> projects,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        List<ERP.Domain.BILL.Entities.Invoice> invoices)
    {
        var performances = new List<ProjectPerformanceDto>();

        foreach (var project in projects)
        {
            var projectTimesheets = timesheets
                .Where(t => t.Status == TimesheetStatus.Approved)
                .SelectMany(t => t.Entries)
                .Where(e => e.ProjectId == project.Id)
                .ToList();

            var plannedHours = project.WBSItems.Sum(w => w.EstimatedHours ?? 0);
            var actualHours = projectTimesheets.Sum(e => e.Hours);
            var actualCost = actualHours * 100m;

            var projectInvoices = invoices.Where(i => i.ProjectId == project.Id).ToList();
            var revenue = projectInvoices
                .Where(i => i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.Paid)
                .Sum(i => i.CalculateTotal().Amount);

            var profit = revenue - actualCost;
            var profitMargin = revenue > 0 ? (profit / revenue) * 100 : 0;
            var budget = project.Budget?.Amount ?? 0;
            var budgetVariance = budget - actualCost;
            var budgetUtilization = budget > 0 ? (actualCost / budget) * 100 : 0;

            performances.Add(new ProjectPerformanceDto
            {
                ProjectId = project.Id,
                ProjectNumber = project.ProjectNumber,
                ProjectName = project.Name,
                ClientName = "Client", // Would join with client
                Status = project.Status,
                Type = project.Type,
                Budget = budget,
                ActualCost = actualCost,
                Revenue = revenue,
                Profit = profit,
                ProfitMargin = profitMargin,
                BudgetVariance = budgetVariance,
                BudgetUtilizationPercent = budgetUtilization,
                PlannedHours = plannedHours,
                ActualHours = actualHours,
                HoursVariance = plannedHours - actualHours,
                IsOverBudget = actualCost > budget,
                IsAtRisk = actualCost > budget || profitMargin < 0,
                DaysUntilDeadline = 30, // Would calculate from actual deadline
                Currency = project.Budget?.Currency ?? "USD"
            });
        }

        return performances;
    }

    private void GenerateChartData(ProjectDashboardDto dashboard, List<ERP.Domain.PM.Entities.Project> projects)
    {
        // Projects by status
        dashboard.ProjectsByStatus = new List<PieChartDataDto>
        {
            new PieChartDataDto { Label = "Active", Value = projects.Count(p => p.Status == ProjectStatus.Active), Percentage = 0 },
            new PieChartDataDto { Label = "On Hold", Value = projects.Count(p => p.Status == ProjectStatus.OnHold), Percentage = 0 },
            new PieChartDataDto { Label = "Completed", Value = projects.Count(p => p.Status == ProjectStatus.Completed), Percentage = 0 }
        };

        var total = dashboard.ProjectsByStatus.Sum(p => p.Value);
        foreach (var item in dashboard.ProjectsByStatus)
        {
            item.Percentage = total > 0 ? (item.Value / total) * 100 : 0;
        }

        // Projects by type
        dashboard.ProjectsByType = new List<PieChartDataDto>
        {
            new PieChartDataDto { Label = "Billable", Value = projects.Count(p => p.Type == ProjectType.Billable), Percentage = 0 },
            new PieChartDataDto { Label = "Overhead", Value = projects.Count(p => p.Type == ProjectType.Overhead), Percentage = 0 },
            new PieChartDataDto { Label = "Proposal", Value = projects.Count(p => p.Type == ProjectType.Proposal), Percentage = 0 }
        };

        total = dashboard.ProjectsByType.Sum(p => p.Value);
        foreach (var item in dashboard.ProjectsByType)
        {
            item.Percentage = total > 0 ? (item.Value / total) * 100 : 0;
        }

        // Budget utilization trend (placeholder)
        dashboard.BudgetUtilizationTrend = new List<ChartDataPointDto>();
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
