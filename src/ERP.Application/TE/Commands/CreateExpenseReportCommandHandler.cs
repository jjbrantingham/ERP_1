using ERP.Application.Common.Interfaces;
using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Repositories;

namespace ERP.Application.TE.Commands;

public class CreateExpenseReportCommandHandler : ICommandHandler<CreateExpenseReportCommand, long>
{
    private readonly IExpenseReportRepository _expenseReportRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateExpenseReportCommandHandler(
        IExpenseReportRepository expenseReportRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _expenseReportRepository = expenseReportRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateExpenseReportCommand command, CancellationToken cancellationToken = default)
    {
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
