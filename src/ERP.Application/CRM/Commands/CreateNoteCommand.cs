using ERP.Application.Common.Interfaces;
using ERP.Domain.CRM.Enums;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Command to create a new note for a client.
/// </summary>
public class CreateNoteCommand : ICommand<long>
{
    public long ClientId { get; set; }
    public long? ContactId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NoteType NoteType { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public long? AuthorId { get; set; }
}
