using ERP.Application.RPT.DTOs;
using ERP.Domain.PM.Repositories;
using ERP.Domain.TE.Repositories;
using ERP.Domain.BILL.Repositories;
using ERP.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.RPT.Queries;

public class GetProjectProfitabilityQueryHandler : IRequestHandler<GetProjectProfitabilityQuery, ProjectProfitabilityDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ERPDbContext _context;

    public GetProjectProfitabilityQueryHandler(
        IProjectRepository projectRepository,
        ITimesheetRepository timesheetRepository,
        IInvoiceRepository invoiceRepository,
        ERPDbContext context)
    {
        _projectRepository = projectRepository;
        _timesheetRepository = timesheetRepository;
        _invoiceRepository = invoiceRepository;
        _context = context;
    }

    public async Task<ProjectProfitabilityDto> Handle(GetProjectProfitabilityQuery request, CancellationToken cancellationToken)
    {
        // Get projects
        var projects = await _projectRepository.GetAllAsync(cancellationToken);

        // Apply filters
        if (request.ClientId.HasValue)
            projects = projects.Where(p => p.ClientId == request.ClientId.Value);

        var result = new ProjectProfitabilityDto();

        foreach (var project in projects)
        {
            // Get timesheets for the project
            var timesheets = await _context.Timesheets
                .Include(t => t.Entries)
                .Where(t => t.Entries.Any(e => e.ProjectId == project.Id))
                .ToListAsync(cancellationToken);

            var projectTimesheetEntries = timesheets
                .SelectMany(t => t.Entries)
                .Where(e => e.ProjectId == project.Id)
                .ToList();

            var actualHours = projectTimesheetEntries.Sum(e => e.Hours);

            // Get invoices for the project
            var invoices = await _invoiceRepository.GetByProjectIdAsync(project.Id, cancellationToken);
            var actualRevenue = invoices
                .Where(i => i.Status == ERP.Domain.BILL.Enums.InvoiceStatus.Posted ||
                           i.Status == ERP.Domain.BILL.Enums.InvoiceStatus.Paid)
                .Sum(i => i.CalculateTotal().Amount);

            // Calculate costs (simplified - would need rate information)
            var actualCost = actualHours * 100m; // Placeholder rate

            var line = new ProjectProfitabilityLineDto
            {
                ProjectId = project.Id,
                ProjectNumber = project.ProjectNumber.Value,
                ProjectName = project.Name,
                ClientName = "Client", // Would need to join with Client
                Status = project.Status.ToString(),
                BillingMode = "T&M", // Would need from project
                ActualHours = actualHours,
                ActualCost = actualCost,
                ActualRevenue = actualRevenue,
                GrossProfit = actualRevenue - actualCost,
                GrossProfitMargin = actualRevenue > 0 ? (actualRevenue - actualCost) / actualRevenue * 100 : 0m
            };

            result.Projects.Add(line);
        }

        // Calculate summary
        result.Summary = new ProjectProfitabilitySummaryDto
        {
            TotalProjects = result.Projects.Count,
            TotalActualRevenue = result.Projects.Sum(p => p.ActualRevenue),
            TotalActualCost = result.Projects.Sum(p => p.ActualCost),
            TotalGrossProfit = result.Projects.Sum(p => p.GrossProfit)
        };

        result.Summary.OverallGrossProfitMargin = result.Summary.TotalActualRevenue > 0
            ? result.Summary.TotalGrossProfit / result.Summary.TotalActualRevenue * 100
            : 0m;

        return result;
    }
}
