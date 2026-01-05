using ERP.Application.Common.Interfaces;
using ERP.Application.Common.Security;
using ERP.Application.HR.DTOs;
using ERP.Domain.HR.Repositories;

namespace ERP.Application.HR.Queries;

/// <summary>
/// Handler for GetEmployeeRatesQuery.
/// </summary>
public class GetEmployeeRatesQueryHandler : IQueryHandler<GetEmployeeRatesQuery, IEnumerable<RateDto>>
{
    private readonly IRateRepository _rateRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantService _currentTenant;

    public GetEmployeeRatesQueryHandler(IRateRepository rateRepository, IEmployeeRepository employeeRepository,
        ICurrentUserService currentUser,
        ICurrentTenantService currentTenant)
    {
        _rateRepository = rateRepository;
        _employeeRepository = employeeRepository;
        _currentUser = currentUser;
        _currentTenant = currentTenant;
    }

    public async Task<IEnumerable<RateDto>> Handle(GetEmployeeRatesQuery query, CancellationToken cancellationToken = default)
    {
        // Ensure user is authenticated
        AuthorizationHelper.EnsureAuthenticated(_currentUser);

        var rates = await _rateRepository.GetByEmployeeIdAsync(query.EmployeeId, cancellationToken);

        return rates.Select(rate => new RateDto
        {
            Id = rate.Id,
            EmployeeId = rate.EmployeeId,
            EmployeeName = rate.Employee?.FullName,
            ResourceTypeId = rate.ResourceTypeId,
            ResourceTypeName = rate.ResourceType?.Name,
            RateType = rate.RateType.ToString(),
            CostRateAmount = rate.CostRate.Amount,
            CostRateCurrency = rate.CostRate.Currency,
            BillingRateAmount = rate.BillingRate.Amount,
            BillingRateCurrency = rate.BillingRate.Currency,
            EffectiveDate = rate.EffectiveDate,
            EndDate = rate.EndDate,
            IsActive = rate.IsActive,
            Notes = rate.Notes,
            CreatedDate = rate.CreatedDate,
            ModifiedDate = rate.ModifiedDate
        });
    }
}
