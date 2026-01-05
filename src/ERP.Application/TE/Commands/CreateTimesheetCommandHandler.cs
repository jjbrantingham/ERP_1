using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Domain.TE.Entities;
using ERP.Domain.TE.Repositories;

namespace ERP.Application.TE.Commands;

public class CreateTimesheetCommandHandler : ICommandHandler<CreateTimesheetCommand, long>
{
    private readonly ITimesheetRepository _timesheetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenantService _currentTenant;
    private readonly ICurrentUserService _currentUser;

    public CreateTimesheetCommandHandler(
        ITimesheetRepository timesheetRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenantService currentTenant,
        ICurrentUserService currentUser)
    {
        _timesheetRepository = timesheetRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<long> Handle(CreateTimesheetCommand command, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUserService);

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
