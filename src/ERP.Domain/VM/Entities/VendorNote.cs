using ERP.Domain.Common;

namespace ERP.Domain.VM.Entities;

/// <summary>
/// Note or interaction record for a vendor.
/// </summary>
public class VendorNote : AggregateRoot
{
    public long VendorId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime NoteDate { get; private set; }
    public long? CreatedByUserId { get; private set; }

    // Navigation property
    public Vendor Vendor { get; private set; } = null!;

    private VendorNote() { } // EF Core

    private VendorNote(
        Guid tenantId,
        long vendorId,
        string subject,
        string content,
        DateTime noteDate,
        long? createdByUserId)
    {
        TenantId = tenantId;
        VendorId = vendorId;
        Subject = subject;
        Content = content;
        NoteDate = noteDate;
        CreatedByUserId = createdByUserId;
        CreatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new vendor note.
    /// </summary>
    public static VendorNote Create(
        Guid tenantId,
        long vendorId,
        string subject,
        string content,
        DateTime? noteDate = null,
        long? createdByUserId = null)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required", nameof(subject));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));

        var note = new VendorNote(
            tenantId,
            vendorId,
            subject,
            content,
            noteDate ?? DateTime.UtcNow,
            createdByUserId);

        return note;
    }

    /// <summary>
    /// Updates the note content.
    /// </summary>
    public void UpdateContent(string subject, string content)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required", nameof(subject));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));

        Subject = subject;
        Content = content;
        ModifiedDate = DateTime.UtcNow;
    }
}
