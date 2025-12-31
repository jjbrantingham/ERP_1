using ERP.Domain.HR.Entities;
using ERP.Domain.HR.Enums;
using ERP.Domain.HR.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Rate entity.
/// </summary>
public class RateRepository : Repository<Rate>, IRateRepository
{
    public RateRepository(ERPDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Rate>> GetByEmployeeIdAsync(long employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Rates
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.EffectiveDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Rate>> GetByResourceTypeIdAsync(long resourceTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.Rates
            .Where(r => r.ResourceTypeId == resourceTypeId)
            .OrderByDescending(r => r.EffectiveDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Rate?> GetCurrentRateForEmployeeAsync(long employeeId, RateType rateType, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Rates
            .Where(r => r.EmployeeId == employeeId &&
                       r.RateType == rateType &&
                       r.IsActive &&
                       r.EffectiveDate <= today &&
                       (r.EndDate == null || r.EndDate >= today))
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Rate?> GetCurrentRateForResourceTypeAsync(long resourceTypeId, RateType rateType, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.Rates
            .Where(r => r.ResourceTypeId == resourceTypeId &&
                       r.RateType == rateType &&
                       r.IsActive &&
                       r.EffectiveDate <= today &&
                       (r.EndDate == null || r.EndDate >= today))
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Rate>> GetActiveRatesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Rates
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.EffectiveDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Rate>> GetRatesEffectiveOnAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.Rates
            .Where(r => r.IsActive &&
                       r.EffectiveDate <= date &&
                       (r.EndDate == null || r.EndDate >= date))
            .OrderByDescending(r => r.EffectiveDate)
            .ToListAsync(cancellationToken);
    }
}
