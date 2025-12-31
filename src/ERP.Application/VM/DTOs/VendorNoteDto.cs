namespace ERP.Application.VM.DTOs;

/// <summary>
/// Data transfer object for Vendor Note.
/// </summary>
public class VendorNoteDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long VendorId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime NoteDate { get; set; }
    public long? CreatedByUserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
