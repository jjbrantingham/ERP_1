using ERP.Domain.Common;
using ERP.Domain.CRM.Enums;

namespace ERP.Domain.CRM.Entities;

/// <summary>
/// Represents a note or interaction record for a client.
/// </summary>
public class Note : Entity
{
    public long ClientId { get; private set; }
    public long? ContactId { get; private set; }
    public NoteType NoteType { get; private set; }

    public string Subject { get; private set; }
    public string Content { get; private set; }
    public DateTime NoteDate { get; private set; }

    // Optional follow-up
    public DateTime? FollowUpDate { get; private set; }
    public bool IsFollowUpComplete { get; private set; }

    // Author tracking
    public long? AuthorId { get; private set; } // Employee who created the note

    // Navigation properties
    public Client? Client { get; private set; }
    public Contact? Contact { get; private set; }

    private Note()
    {
        Subject = string.Empty;
        Content = string.Empty;
    }

    /// <summary>
    /// Creates a new note.
    /// </summary>
    public static Note Create(
        Guid tenantId,
        long clientId,
        string subject,
        string content,
        NoteType noteType,
        long? contactId = null,
        long? authorId = null,
        DateTime? followUpDate = null)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required", nameof(subject));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));

        var note = new Note
        {
            TenantId = tenantId,
            ClientId = clientId,
            ContactId = contactId,
            NoteType = noteType,
            Subject = subject.Trim(),
            Content = content.Trim(),
            NoteDate = DateTime.UtcNow,
            FollowUpDate = followUpDate,
            IsFollowUpComplete = false,
            AuthorId = authorId,
            CreatedDate = DateTime.UtcNow
        };

        return note;
    }

    /// <summary>
    /// Updates the note.
    /// </summary>
    public void Update(
        string subject,
        string content,
        NoteType noteType,
        DateTime? followUpDate = null)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required", nameof(subject));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));

        Subject = subject.Trim();
        Content = content.Trim();
        NoteType = noteType;
        FollowUpDate = followUpDate;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the follow-up as complete.
    /// </summary>
    public void CompleteFollowUp()
    {
        IsFollowUpComplete = true;
        ModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets or updates the follow-up date.
    /// </summary>
    public void SetFollowUpDate(DateTime followUpDate)
    {
        FollowUpDate = followUpDate;
        IsFollowUpComplete = false;
        ModifiedDate = DateTime.UtcNow;
    }
}
