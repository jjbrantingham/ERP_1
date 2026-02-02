using ERP.Domain.Common;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.Repositories;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure.Persistence;

namespace ERP.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Contract entity.
/// </summary>
public class ContractRepository : Repository<Contract>, IContractRepository
{
    public ContractRepository(ERPDbContext context) : base(context)
    {
    }

    public override async Task<Contract?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Contract?> GetByContractNumberAsync(string contractNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber, cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Where(c => c.ProjectId == projectId)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetActiveContractsAsync(long projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Where(c => c.ProjectId == projectId && c.IsActive)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetByTypeAsync(ContractType contractType, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Where(c => c.ContractType == contractType)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetExpiringContractsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Where(c => c.IsActive && c.EndDate >= startDate && c.EndDate <= endDate)
            .OrderBy(c => c.EndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string contractNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .AnyAsync(c => c.ContractNumber == contractNumber, cancellationToken);
    }

    public async Task<IEnumerable<Contract>> GetAllWithProjectAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Include(c => c.Project)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Contract?> GetByIdWithProjectAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Contracts
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
