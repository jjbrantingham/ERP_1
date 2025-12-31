namespace ERP.Application.CRM.DTOs;

/// <summary>
/// Data transfer object for Note.
/// </summary>
public class NoteDto
{
    public long Id { get; set; }
    public long ClientId { get; set; }
    public string? ClientName { get; set; }
    public long? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string NoteType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime NoteDate { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public bool IsFollowUpComplete { get; set; }
    public long? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
