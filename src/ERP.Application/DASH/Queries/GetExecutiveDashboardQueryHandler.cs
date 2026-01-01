using ERP.Application.Common.Interfaces;
using ERP.Application.DASH.DTOs;
using ERP.Domain.CRM.Repositories;
using ERP.Domain.DASH.Enums;
using ERP.Domain.FIN.Enums;
using ERP.Domain.FIN.Repositories;
using ERP.Domain.HR.Repositories;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using ERP.Domain.TE.Enums;
using ERP.Domain.TE.Repositories;
using ERP.Domain.BILL.Enums;
using ERP.Domain.BILL.Repositories;
using MediatR;

namespace ERP.Application.DASH.Queries;

/// <summary>
/// Handler for getting executive dashboard data
/// </summary>
public class GetExecutiveDashboardQueryHandler : IRequestHandler<GetExecutiveDashboardQuery, ExecutiveDashboardDto>
{
    private readonly ICurrentTenantService _currentTenant;
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public GetExecutiveDashboardQueryHandler(
        ICurrentTenantService currentTenant,
        IProjectRepository projectRepository,
        IEmployeeRepository employeeRepository,
        IClientRepository clientRepository,
        ITimesheetRepository timesheetRepository,
        IAccountRepository accountRepository,
        IInvoiceRepository invoiceRepository)
    {
        _currentTenant = currentTenant;
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
        _clientRepository = clientRepository;
        _timesheetRepository = timesheetRepository;
        _accountRepository = accountRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<ExecutiveDashboardDto> Handle(GetExecutiveDashboardQuery request, CancellationToken cancellationToken)
    {
        var asOfDate = request.AsOfDate ?? DateTime.UtcNow;
        var currentMonth = new DateTime(asOfDate.Year, asOfDate.Month, 1);
        var previousMonth = currentMonth.AddMonths(-1);
        var currency = request.Currency;

        var dashboard = new ExecutiveDashboardDto
        {
            AsOfDate = asOfDate,
            TenantId = _currentTenant.TenantId
        };

        // Get all data in parallel
        var accountsTask = _accountRepository.GetAllAsync(cancellationToken);
        var projectsTask = _projectRepository.GetAllAsync(cancellationToken);
        var employeesTask = _employeeRepository.GetAllAsync(cancellationToken);
        var clientsTask = _clientRepository.GetAllAsync(cancellationToken);
        var timesheetsTask = _timesheetRepository.GetAllAsync(cancellationToken);
        var invoicesTask = _invoiceRepository.GetAllAsync(cancellationToken);

        await Task.WhenAll(accountsTask, projectsTask, employeesTask, clientsTask, timesheetsTask, invoicesTask);

        var accounts = accountsTask.Result.ToList();
        var projects = projectsTask.Result.ToList();
        var employees = employeesTask.Result.ToList();
        var clients = clientsTask.Result.ToList();
        var timesheets = timesheetsTask.Result.ToList();
        var invoices = invoicesTask.Result.ToList();

        // Calculate Financial Metrics
        await CalculateFinancialMetrics(dashboard, accounts, invoices, currentMonth, previousMonth, currency);

        // Calculate Project Metrics
        await CalculateProjectMetrics(dashboard, projects, timesheets, invoices);

        // Calculate Employee Metrics
        await CalculateEmployeeMetrics(dashboard, employees, timesheets, currentMonth);

        // Calculate Client Metrics
        await CalculateClientMetrics(dashboard, clients, currentMonth);

        // Get top performing projects
        dashboard.TopProjects = GetTopProjects(projects, timesheets, invoices, 5);

        // Get recent activities (placeholder - would integrate with event/audit log)
        dashboard.RecentActivities = new List<ActivityHighlightDto>();

        return dashboard;
    }

    private Task CalculateFinancialMetrics(
        ExecutiveDashboardDto dashboard,
        List<ERP.Domain.FIN.Entities.Account> accounts,
        List<ERP.Domain.BILL.Entities.Invoice> invoices,
        DateTime currentMonth,
        DateTime previousMonth,
        string currency)
    {
        // Monthly Revenue
        var currentMonthRevenue = invoices
            .Where(i => i.InvoiceDate >= currentMonth && i.InvoiceDate < currentMonth.AddMonths(1) &&
                       (i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.Paid))
            .Sum(i => i.CalculateTotal().Amount);

        var previousMonthRevenue = invoices
            .Where(i => i.InvoiceDate >= previousMonth && i.InvoiceDate < currentMonth &&
                       (i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.Paid))
            .Sum(i => i.CalculateTotal().Amount);

        dashboard.MonthlyRevenue = CreateKpiMetric(
            KpiType.Revenue,
            "Monthly Revenue",
            currentMonthRevenue,
            previousMonthRevenue,
            null,
            currency,
            "currency");

        // Monthly Profit (Revenue - Expenses)
        var currentMonthExpenses = accounts
            .Where(a => a.Type == AccountType.Expense)
            .Sum(a => a.Balance);

        var currentMonthProfit = currentMonthRevenue - currentMonthExpenses;
        var previousMonthProfit = previousMonthRevenue - (currentMonthExpenses * 0.9m); // Estimate

        dashboard.MonthlyProfit = CreateKpiMetric(
            KpiType.Profit,
            "Monthly Profit",
            currentMonthProfit,
            previousMonthProfit,
            null,
            currency,
            "currency");

        // Profit Margin
        var profitMargin = currentMonthRevenue > 0 ? (currentMonthProfit / currentMonthRevenue) * 100 : 0;
        var previousProfitMargin = previousMonthRevenue > 0 ? (previousMonthProfit / previousMonthRevenue) * 100 : 0;

        dashboard.ProfitMargin = CreateKpiMetric(
            KpiType.Profit,
            "Profit Margin",
            profitMargin,
            previousProfitMargin,
            null,
            currency,
            "percentage");

        // Cash Balance
        var cashBalance = accounts
            .Where(a => a.Type == AccountType.Asset && a.AccountNumber.Value.StartsWith("10"))
            .Sum(a => a.Balance);

        dashboard.CashBalance = CreateKpiMetric(
            KpiType.CashFlow,
            "Cash Balance",
            cashBalance,
            null,
            null,
            currency,
            "currency");

        // Accounts Receivable
        var accountsReceivable = accounts
            .Where(a => a.Type == AccountType.Asset && a.AccountNumber.Value.StartsWith("12"))
            .Sum(a => a.Balance);

        dashboard.AccountsReceivable = CreateKpiMetric(
            KpiType.AccountsReceivable,
            "Accounts Receivable",
            accountsReceivable,
            null,
            null,
            currency,
            "currency");

        // Accounts Payable
        var accountsPayable = accounts
            .Where(a => a.Type == AccountType.Liability && a.AccountNumber.Value.StartsWith("20"))
            .Sum(a => a.Balance);

        dashboard.AccountsPayable = CreateKpiMetric(
            KpiType.AccountsPayable,
            "Accounts Payable",
            accountsPayable,
            null,
            null,
            currency,
            "currency");

        return Task.CompletedTask;
    }

    private Task CalculateProjectMetrics(
        ExecutiveDashboardDto dashboard,
        List<ERP.Domain.PM.Entities.Project> projects,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        List<ERP.Domain.BILL.Entities.Invoice> invoices)
    {
        var activeProjects = projects.Where(p => p.Status == ProjectStatus.Active).ToList();
        var onBudgetCount = 0;
        var overBudgetCount = 0;
        var totalMargin = 0m;

        foreach (var project in activeProjects)
        {
            var projectTimesheets = timesheets
                .Where(t => t.Status == TimesheetStatus.Approved)
                .SelectMany(t => t.Entries)
                .Where(e => e.ProjectId == project.Id)
                .ToList();

            var actualCost = projectTimesheets.Sum(e => e.Hours) * 100m; // Simplified cost calculation
            var projectInvoices = invoices.Where(i => i.ProjectId == project.Id).ToList();
            var revenue = projectInvoices.Sum(i => i.CalculateTotal().Amount);

            if (actualCost <= (project.Budget?.Amount ?? 0))
                onBudgetCount++;
            else
                overBudgetCount++;

            if (revenue > 0)
                totalMargin += (revenue - actualCost) / revenue;
        }

        dashboard.ActiveProjects = CreateKpiMetric(
            KpiType.ActiveProjects,
            "Active Projects",
            activeProjects.Count,
            null,
            null,
            null,
            "number");

        dashboard.ProjectsOnBudget = CreateKpiMetric(
            KpiType.ActiveProjects,
            "Projects On Budget",
            onBudgetCount,
            null,
            null,
            null,
            "number");

        dashboard.ProjectsOverBudget = CreateKpiMetric(
            KpiType.OverBudgetProjects,
            "Projects Over Budget",
            overBudgetCount,
            null,
            null,
            null,
            "number");

        var avgMargin = activeProjects.Count > 0 ? (totalMargin / activeProjects.Count) * 100 : 0;
        dashboard.AverageProjectMargin = CreateKpiMetric(
            KpiType.ProjectProfitability,
            "Average Project Margin",
            avgMargin,
            null,
            null,
            null,
            "percentage");

        return Task.CompletedTask;
    }

    private Task CalculateEmployeeMetrics(
        ExecutiveDashboardDto dashboard,
        List<ERP.Domain.HR.Entities.Employee> employees,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        DateTime currentMonth)
    {
        var activeEmployees = employees.Where(e => e.Status == Domain.HR.Enums.EmployeeStatus.Active).ToList();

        var currentMonthTimesheets = timesheets
            .Where(t => t.PeriodStart >= currentMonth && t.PeriodStart < currentMonth.AddMonths(1) &&
                       t.Status == TimesheetStatus.Approved)
            .ToList();

        var totalHours = currentMonthTimesheets.SelectMany(t => t.Entries).Sum(e => e.Hours);
        var billableHours = currentMonthTimesheets.SelectMany(t => t.Entries).Where(e => e.IsBillable).Sum(e => e.Hours);
        var nonBillableHours = totalHours - billableHours;

        var utilization = totalHours > 0 ? (billableHours / totalHours) * 100 : 0;

        dashboard.TotalEmployees = CreateKpiMetric(
            KpiType.ActiveProjects,
            "Total Employees",
            activeEmployees.Count,
            null,
            null,
            null,
            "number");

        dashboard.EmployeeUtilization = CreateKpiMetric(
            KpiType.EmployeeUtilization,
            "Employee Utilization",
            utilization,
            null,
            70m, // Target 70% utilization
            null,
            "percentage");

        dashboard.BillableHours = CreateKpiMetric(
            KpiType.BillableHours,
            "Billable Hours",
            billableHours,
            null,
            null,
            null,
            "hours");

        dashboard.NonBillableHours = CreateKpiMetric(
            KpiType.NonBillableHours,
            "Non-Billable Hours",
            nonBillableHours,
            null,
            null,
            null,
            "hours");

        return Task.CompletedTask;
    }

    private Task CalculateClientMetrics(
        ExecutiveDashboardDto dashboard,
        List<ERP.Domain.CRM.Entities.Client> clients,
        DateTime currentMonth)
    {
        var activeClients = clients.Where(c => c.Status == Domain.CRM.Enums.ClientStatus.Active).Count();
        var newClients = clients.Where(c => c.CreatedDate >= currentMonth && c.CreatedDate < currentMonth.AddMonths(1)).Count();

        dashboard.ActiveClients = CreateKpiMetric(
            KpiType.ActiveProjects,
            "Active Clients",
            activeClients,
            null,
            null,
            null,
            "number");

        dashboard.NewClientsThisMonth = CreateKpiMetric(
            KpiType.ActiveProjects,
            "New Clients This Month",
            newClients,
            null,
            null,
            null,
            "number");

        // Client retention rate (placeholder)
        dashboard.ClientRetentionRate = CreateKpiMetric(
            KpiType.ClientSatisfaction,
            "Client Retention Rate",
            95m,
            null,
            90m,
            null,
            "percentage");

        return Task.CompletedTask;
    }

    private List<ProjectPerformanceDto> GetTopProjects(
        List<ERP.Domain.PM.Entities.Project> projects,
        List<ERP.Domain.TE.Entities.Timesheet> timesheets,
        List<ERP.Domain.BILL.Entities.Invoice> invoices,
        int count)
    {
        var projectPerformances = new List<ProjectPerformanceDto>();

        foreach (var project in projects.Where(p => p.Status == ProjectStatus.Active))
        {
            var projectTimesheets = timesheets
                .Where(t => t.Status == TimesheetStatus.Approved)
                .SelectMany(t => t.Entries)
                .Where(e => e.ProjectId == project.Id)
                .ToList();

            var actualHours = projectTimesheets.Sum(e => e.Hours);
            var actualCost = actualHours * 100m; // Simplified

            var projectInvoices = invoices.Where(i => i.ProjectId == project.Id).ToList();
            var revenue = projectInvoices
                .Where(i => i.Status == InvoiceStatus.Posted || i.Status == InvoiceStatus.Paid)
                .Sum(i => i.CalculateTotal().Amount);

            var profit = revenue - actualCost;
            var profitMargin = revenue > 0 ? (profit / revenue) * 100 : 0;

            projectPerformances.Add(new ProjectPerformanceDto
            {
                ProjectId = project.Id,
                ProjectNumber = project.ProjectNumber,
                ProjectName = project.Name,
                ClientName = "Client", // Would need to join with client
                Status = project.Status,
                Type = project.Type,
                Budget = project.Budget?.Amount ?? 0,
                ActualCost = actualCost,
                Revenue = revenue,
                Profit = profit,
                ProfitMargin = profitMargin,
                ActualHours = actualHours,
                Currency = project.Budget?.Currency ?? "USD"
            });
        }

        return projectPerformances
            .OrderByDescending(p => p.Profit)
            .Take(count)
            .ToList();
    }

    private KpiMetricDto CreateKpiMetric(
        KpiType type,
        string name,
        decimal currentValue,
        decimal? previousValue,
        decimal? targetValue,
        string? currency,
        string format)
    {
        var percentageChange = previousValue.HasValue && previousValue.Value != 0
            ? ((currentValue - previousValue.Value) / previousValue.Value) * 100
            : (decimal?)null;

        var trend = TrendDirection.Flat;
        if (percentageChange.HasValue)
        {
            if (percentageChange.Value > 5) trend = TrendDirection.Up;
            else if (percentageChange.Value < -5) trend = TrendDirection.Down;
        }

        var isHealthy = true;
        if (targetValue.HasValue)
        {
            isHealthy = currentValue >= targetValue.Value;
        }
        else if (percentageChange.HasValue)
        {
            // For revenue/profit, up is good
            isHealthy = percentageChange.Value >= 0;
        }

        return new KpiMetricDto
        {
            Type = type,
            Name = name,
            CurrentValue = currentValue,
            PreviousValue = previousValue,
            TargetValue = targetValue,
            PercentageChange = percentageChange.HasValue ? Math.Round(percentageChange.Value, 2) : null,
            Trend = trend,
            Currency = currency,
            Format = format,
            IsHealthy = isHealthy
        };
    }
}
