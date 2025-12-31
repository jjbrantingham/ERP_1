namespace ERP.Application.VM.Commands;

/// <summary>
/// Command to create a new vendor note.
/// </summary>
public class CreateVendorNoteCommand
{
    public long VendorId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? NoteDate { get; set; }
}
