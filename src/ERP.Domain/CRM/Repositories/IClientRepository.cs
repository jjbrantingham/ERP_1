using ERP.Application.Common.Interfaces;
using ERP.Domain.CRM.Entities;
using ERP.Domain.CRM.Enums;

namespace ERP.Domain.CRM.Repositories;

/// <summary>
/// Repository interface for Client aggregate.
/// </summary>
public interface IClientRepository : IRepository<Client>
{
    /// <summary>
    /// Gets a client by ID with contacts and notes included.
    /// </summary>
    Task<Client?> GetByIdWithDetailsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a client by client number.
    /// </summary>
    Task<Client?> GetByClientNumberAsync(string clientNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a client by email address.
    /// </summary>
    Task<Client?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active clients.
    /// </summary>
    Task<IEnumerable<Client>> GetActiveClientsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets clients by status.
    /// </summary>
    Task<IEnumerable<Client>> GetByStatusAsync(ClientStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets clients by type.
    /// </summary>
    Task<IEnumerable<Client>> GetByTypeAsync(ClientType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets clients by industry.
    /// </summary>
    Task<IEnumerable<Client>> GetByIndustryAsync(string industry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets clients by account manager.
    /// </summary>
    Task<IEnumerable<Client>> GetByAccountManagerAsync(long accountManagerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches clients by name or client number.
    /// </summary>
    Task<IEnumerable<Client>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
}
