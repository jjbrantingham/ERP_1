using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Repositories;

namespace ERP.Application.TE.Commands;

public class CreateExpenseReportCommandHandler : ICommandHandler<CreateExpenseReportCommand, long>
{
    private readonly IExpenseReportRepository _expenseReportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;

    public CreateExpenseReportCommandHandler(
        IExpenseReportRepository expenseReportRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser)
    {
        _expenseReportRepository = expenseReportRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<long> Handle(CreateExpenseReportCommand command, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

        var reportNumber = $"EXP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..26];

        var expenseReport = ExpenseReport.Create(
            _currentTenant.TenantId,
            command.EmployeeId,
            reportNumber,
            command.ReportDate,
            command.Purpose,
            command.Notes
        );

        await _expenseReportRepository.AddAsync(expenseReport, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return expenseReport.Id;
    }
}
