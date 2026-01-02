namespace ERP.Application.Common.Interfaces;

/// <summary>
/// Service for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email
    /// </summary>
    Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email to multiple recipients
    /// </summary>
    Task SendEmailAsync(
        IEnumerable<string> to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email using a template
    /// </summary>
    Task SendTemplatedEmailAsync<T>(
        string to,
        string templateName,
        T model,
        CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Email template model for workflow notifications
/// </summary>
public class WorkflowNotificationEmailModel
{
    public string WorkflowName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string ActionRequired { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}
