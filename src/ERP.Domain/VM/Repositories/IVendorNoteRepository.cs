using ERP.Domain.Common.Interfaces;
using ERP.Domain.VM.Entities;

namespace ERP.Domain.VM.Repositories;

/// <summary>
/// Repository interface for VendorNote entity.
/// </summary>
public interface IVendorNoteRepository : IRepository<VendorNote>
{
    /// <summary>
    /// Get note by ID.
    /// </summary>
    Task<VendorNote?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all notes for a vendor.
    /// </summary>
    Task<IEnumerable<VendorNote>> GetByVendorIdAsync(long vendorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent notes for a vendor.
    /// </summary>
    Task<IEnumerable<VendorNote>> GetRecentNotesAsync(long vendorId, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search notes by content.
    /// </summary>
    Task<IEnumerable<VendorNote>> SearchAsync(long vendorId, string searchTerm, CancellationToken cancellationToken = default);
}
