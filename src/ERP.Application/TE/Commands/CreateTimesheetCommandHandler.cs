using ERP.Application.Common.Interfaces;
using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Repositories;

namespace ERP.Application.TE.Commands;

public class CreateTimesheetCommandHandler : ICommandHandler<CreateTimesheetCommand, long>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;

    public CreateTimesheetCommandHandler(
        ITimesheetRepository timesheetRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant)
    {
        _timesheetRepository = timesheetRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<long> Handle(CreateTimesheetCommand command, CancellationToken cancellationToken = default)
    {
        var timesheet = Timesheet.Create(
            _currentTenant.TenantId,
            command.EmployeeId,
            command.PeriodStart,
            command.PeriodEnd,
            command.Notes
        );

        await _timesheetRepository.AddAsync(timesheet, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return timesheet.Id;
    }
}
