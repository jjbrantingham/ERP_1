using ERP.Application.Common.Interfaces;
using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Enums;

namespace ERP.Domain.HR.Repositories;

/// <summary>
/// Repository interface for Rate entity.
/// </summary>
public interface IRateRepository : IRepository<Rate>
{
    /// <summary>
    /// Gets all rates for an employee.
    /// </summary>
    Task<IEnumerable<Rate>> GetByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all rates for a resource type.
    /// </summary>
    Task<IEnumerable<Rate>> GetByResourceTypeIdAsync(long resourceTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current effective rate for an employee and rate type.
    /// </summary>
    Task<Rate?> GetCurrentRateForEmployeeAsync(long employeeId, RateType rateType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current effective rate for a resource type and rate type.
    /// </summary>
    Task<Rate?> GetCurrentRateForResourceTypeAsync(long resourceTypeId, RateType rateType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active rates.
    /// </summary>
    Task<IEnumerable<Rate>> GetActiveRatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets rates effective on a specific date.
    /// </summary>
    Task<IEnumerable<Rate>> GetRatesEffectiveOnAsync(DateTime date, CancellationToken cancellationToken = default);
}
