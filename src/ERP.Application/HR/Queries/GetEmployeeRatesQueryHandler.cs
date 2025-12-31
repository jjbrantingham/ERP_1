using ERP.Application.Common.Interfaces;
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

    public GetEmployeeRatesQueryHandler(IRateRepository rateRepository, IEmployeeRepository employeeRepository)
    {
        _rateRepository = rateRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<IEnumerable<RateDto>> Handle(GetEmployeeRatesQuery query, CancellationToken cancellationToken = default)
    {
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
