using ERP.Application.AUDIT.Commands;
using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.HR.Repositories;
using ERP.Domain.TE.Repositories;
using ERP.Domain.AUDIT.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERP.Application.AUDIT.Handlers;

/// <summary>
/// Handler for exporting all user data (GDPR Right to Data Portability).
/// </summary>
public class ExportUserDataCommandHandler : IRequestHandler<ExportUserDataCommand, string>
{
    private readonly IDbContext _context;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;

    public ExportUserDataCommandHandler(
        IDbContext context,
        IEmployeeRepository employeeRepository,
        ITimesheetRepository timesheetRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _employeeRepository = employeeRepository;
        _timesheetRepository = timesheetRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(ExportUserDataCommand request, CancellationToken cancellationToken)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        var exportData = new Dictionary<string, object>();

        // Export employee/user basic data
        var employee = await _employeeRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (employee != null)
        {
            exportData["Employee"] = new
            {
                employee.Id,
                employee.EmployeeNumber,
                employee.FirstName,
                employee.LastName,
                employee.Email,
                Phone = employee.PhoneNumber,
                Title = employee.JobTitle,
                employee.Department,
                Status = employee.Status,
                employee.HireDate,
                employee.TerminationDate
            };
        }

        // Export user identity data
        var user = await _context.Users
            .Where(u => u.Id == request.UserId)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Email = u.Email.Value,
                u.EmailConfirmed,
                u.IsActive,
                u.LastLoginDate,
                u.CreatedDate,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user != null)
        {
            exportData["User"] = user;
        }

        // Export timesheets
        var timesheets = await _context.Timesheets
            .Where(t => t.EmployeeId == request.UserId)
            .Include(t => t.Entries)
            .Select(t => new
            {
                t.Id,
                WeekStartDate = t.PeriodStart,
                WeekEndDate = t.PeriodEnd,
                t.Status,
                t.TotalHours,
                t.SubmittedDate,
                t.ApprovedDate,
                Entries = t.Entries.Select(e => new
                {
                    e.Id,
                    e.WorkDate,
                    e.Hours,
                    e.ProjectId,
                    e.WBSItemId,
                    e.Description
                })
            })
            .ToListAsync(cancellationToken);

        exportData["Timesheets"] = timesheets;

        // Export expense reports
        var expenseReports = await _context.ExpenseReports
            .Where(e => e.EmployeeId == request.UserId)
            .Include(e => e.Items)
            .Select(e => new
            {
                e.Id,
                e.ReportNumber,
                e.ReportDate,
                e.Status,
                e.TotalAmount,
                e.SubmittedDate,
                e.ApprovedDate,
                Items = e.Items.Select(i => new
                {
                    i.Id,
                    i.ExpenseDate,
                    i.Amount,
                    i.Category,
                    i.Description,
                    i.Vendor
                })
            })
            .ToListAsync(cancellationToken);

        exportData["ExpenseReports"] = expenseReports;

        // Export audit logs related to this user
        var auditLogs = await _auditLogRepository.GetByUserAsync(
            request.UserId,
            fromDate: null,
            toDate: null,
            cancellationToken: cancellationToken
        );

        exportData["AuditLogs"] = auditLogs.Select(a => new
        {
            a.Id,
            a.EventType,
            a.Severity,
            a.EntityType,
            a.EntityId,
            a.Description,
            a.Timestamp,
            a.IpAddress
        }).ToList();

        // Export notes/comments created by the user
        var notes = await _context.Notes
            .Where(n => n.CreatedBy == user!.Username)
            .Select(n => new
            {
                n.Id,
                n.Content,
                n.ClientId,
                n.CreatedDate
            })
            .ToListAsync(cancellationToken);

        exportData["Notes"] = notes;

        // Add export metadata
        exportData["ExportMetadata"] = new
        {
            ExportDate = DateTime.UtcNow,
            UserId = request.UserId,
            DataCategories = exportData.Keys.Where(k => k != "ExportMetadata").ToList(),
            Purpose = "GDPR Right to Data Portability - Article 20"
        };

        // Serialize to JSON
        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return json;
    }
}
