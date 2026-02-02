using ERP.Domain.Common;
using ERP.Domain.Common.Interfaces;
using ERP.Domain.PM.Entities;

namespace ERP.Domain.PM.Repositories;

/// <summary>
/// Repository interface for WBS Item entity.
/// </summary>
public interface IWBSItemRepository : IRepository<WBSItem>
{
    /// <summary>
    /// Get WBS item by ID.
    /// </summary>
    new Task<WBSItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all WBS items for a project.
    /// </summary>
    Task<IEnumerable<WBSItem>> GetByProjectIdAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all child WBS items for a parent.
    /// </summary>
    Task<IEnumerable<WBSItem>> GetByParentIdAsync(long parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all root-level WBS items for a project (items without parent).
    /// </summary>
    Task<IEnumerable<WBSItem>> GetRootItemsAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get WBS item by WBS code within a project.
    /// </summary>
    Task<WBSItem?> GetByWBSCodeAsync(long projectId, string wbsCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get hierarchical tree of WBS items for a project.
    /// </summary>
    Task<IEnumerable<WBSItem>> GetHierarchicalTreeAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if WBS code exists within a project.
    /// </summary>
    Task<bool> ExistsAsync(long projectId, string wbsCode, CancellationToken cancellationToken = default);
}
