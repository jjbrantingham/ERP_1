using ERP.Domain.Common.Interfaces;
using ERP.Domain.PM.Entities;
using ERP.Domain.PM.Enums;
using ERP.Domain.PM.ValueObjects;

namespace ERP.Domain.PM.Repositories;

/// <summary>
/// Repository interface for Project aggregate root.
/// </summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// Get project by ID with all related entities.
    /// </summary>
    Task<Project?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project by project number.
    /// </summary>
    Task<Project?> GetByProjectNumberAsync(ProjectNumber projectNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all projects for a client.
    /// </summary>
    Task<IEnumerable<Project>> GetByClientIdAsync(long clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all projects by status.
    /// </summary>
    Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active projects.
    /// </summary>
    Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all projects managed by a specific employee.
    /// </summary>
    Task<IEnumerable<Project>> GetByProjectManagerAsync(long projectManagerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all projects by type.
    /// </summary>
    Task<IEnumerable<Project>> GetByTypeAsync(ProjectType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all billable projects.
    /// </summary>
    Task<IEnumerable<Project>> GetBillableProjectsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Search projects by name or project number.
    /// </summary>
    Task<IEnumerable<Project>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get project count by status.
    /// </summary>
    Task<int> GetCountByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if project number exists.
    /// </summary>
    Task<bool> ExistsAsync(ProjectNumber projectNumber, CancellationToken cancellationToken = default);
}
