using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.TE.DTOs;
using ERP.Domain.TE.Repositories;

namespace ERP.Application.TE.Queries;

public class GetExpenseReportByIdQueryHandler : IQueryHandler<GetExpenseReportByIdQuery, ExpenseReportDto>
{
    private readonly IExpenseReportRepository _expenseReportRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetExpenseReportByIdQueryHandler(IExpenseReportRepository expenseReportRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _expenseReportRepository = expenseReportRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<ExpenseReportDto> Handle(GetExpenseReportByIdQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var report = await _expenseReportRepository.GetByIdAsync(query.ExpenseReportId, cancellationToken);
        if (report == null)
            throw new KeyNotFoundException($"Expense report with ID {query.ExpenseReportId} not found.");

        return new ExpenseReportDto
        {
            Id = report.Id,
            TenantId = report.TenantId,
            EmployeeId = report.EmployeeId,
            ReportNumber = report.ReportNumber,
            Purpose = report.Purpose,
            ReportDate = report.ReportDate,
            Status = report.Status.ToString(),
            TotalAmount = report.TotalAmount.Amount,
            Currency = report.TotalAmount.Currency,
            SubmittedDate = report.SubmittedDate,
            ApprovedDate = report.ApprovedDate,
            ApprovedByUserId = report.ApprovedByUserId,
            ApprovalComments = report.ApprovalComments,
            ReimbursedDate = report.ReimbursedDate,
            Notes = report.Notes,
            CreatedDate = report.CreatedDate,
            ModifiedDate = report.ModifiedDate
        };
    }
}
