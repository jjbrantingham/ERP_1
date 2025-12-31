using ERP.Domain.Common.Interfaces;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;

namespace ERP.Domain.PM.Repositories;

/// <summary>
/// Repository interface for Contract entity.
/// </summary>
public interface IContractRepository : IRepository<Contract>
{
    /// <summary>
    /// Get contract by ID.
    /// </summary>
    Task<Contract?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get contract by contract number.
    /// </summary>
    Task<Contract?> GetByContractNumberAsync(string contractNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all contracts for a project.
    /// </summary>
    Task<IEnumerable<Contract>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active contracts for a project.
    /// </summary>
    Task<IEnumerable<Contract>> GetActiveContractsAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all contracts by type.
    /// </summary>
    Task<IEnumerable<Contract>> GetByTypeAsync(ContractType contractType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all contracts expiring within a date range.
    /// </summary>
    Task<IEnumerable<Contract>> GetExpiringContractsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if contract number exists.
    /// </summary>
    Task<bool> ExistsAsync(string contractNumber, CancellationToken cancellationToken = default);
}
