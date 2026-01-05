using ERP.Application.Common.Interfaces;
namespace ERP.Application.VM.Commands;

/// <summary>
/// Command to create a new vendor note.
/// </summary>
public class CreateVendorNoteCommand : ICommand<long>
{
    public long VendorId { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime? NoteDate { get; init; }
}
