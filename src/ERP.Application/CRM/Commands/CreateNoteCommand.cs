using ERP.Application.Common.Interfaces;
using ERP.Domain.CRM.Enums;

namespace ERP.Application.CRM.Commands;

/// <summary>
/// Command to create a new note for a client.
/// </summary>
public class CreateNoteCommand : ICommand<long>
{
    public long ClientId { get; init; }
    public long? ContactId { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public NoteType NoteType { get; init; }
    public DateTime? FollowUpDate { get; init; }
    public long? AuthorId { get; init; }
}
