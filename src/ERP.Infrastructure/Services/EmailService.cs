using ERP.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Services;

/// <summary>
/// Email service implementation
/// NOTE: This is a placeholder implementation that logs emails instead of sending them.
/// In production, replace with actual email provider (SendGrid, AWS SES, SMTP, etc.)
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(new[] { to }, subject, body, isHtml, cancellationToken);
    }

    public async Task SendEmailAsync(
        IEnumerable<string> to,
        string subject,
        string body,
        bool isHtml = true,
        CancellationToken cancellationToken = default)
    {
        // TODO: Replace with actual email provider implementation
        // For now, log the email that would be sent

        _logger.LogInformation(
            "Email would be sent:\nTo: {Recipients}\nSubject: {Subject}\nBody:\n{Body}",
            string.Join(", ", to),
            subject,
            body);

        // Simulate async operation
        await Task.Delay(100, cancellationToken);

        /*
        // Example SendGrid implementation:
        var apiKey = _configuration["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);

        var from = new EmailAddress(
            _configuration["Email:FromAddress"],
            _configuration["Email:FromName"]);

        var msg = MailHelper.CreateSingleEmailToMultipleRecipients(
            from,
            to.Select(email => new EmailAddress(email)).ToList(),
            subject,
            isHtml ? null : body,
            isHtml ? body : null);

        var response = await client.SendEmailAsync(msg, cancellationToken);

        if (response.StatusCode != System.Net.HttpStatusCode.OK &&
            response.StatusCode != System.Net.HttpStatusCode.Accepted)
        {
            _logger.LogError("Failed to send email: {StatusCode}", response.StatusCode);
            throw new Exception($"Failed to send email: {response.StatusCode}");
        }
        */
    }

    public async Task SendTemplatedEmailAsync<T>(
        string to,
        string templateName,
        T model,
        CancellationToken cancellationToken = default) where T : class
    {
        // Load template and render with model
        var template = await LoadTemplateAsync(templateName, cancellationToken);
        var body = RenderTemplate(template, model);
        var subject = ExtractSubject(template, model);

        await SendEmailAsync(to, subject, body, isHtml: true, cancellationToken);
    }

    private async Task<string> LoadTemplateAsync(string templateName, CancellationToken cancellationToken)
    {
        // TODO: Load from file system or embedded resources
        // For now, return a simple template

        var templates = new Dictionary<string, string>
        {
            ["WorkflowStepActivated"] = @"
                <html>
                <body>
                    <h2>Action Required: {WorkflowName}</h2>
                    <p>Hello {RecipientName},</p>
                    <p>You have a pending approval request:</p>
                    <ul>
                        <li><strong>Workflow:</strong> {WorkflowName}</li>
                        <li><strong>Entity:</strong> {EntityType} #{EntityId}</li>
                        <li><strong>Step:</strong> {StepName}</li>
                        <li><strong>Action:</strong> {ActionRequired}</li>
                        <li><strong>Due Date:</strong> {DueDate}</li>
                    </ul>
                    <p><a href='{ActionUrl}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Review Now</a></p>
                    <p>Thank you,<br/>ERP System</p>
                </body>
                </html>",

            ["WorkflowCompleted"] = @"
                <html>
                <body>
                    <h2>Workflow Completed: {WorkflowName}</h2>
                    <p>Hello {RecipientName},</p>
                    <p>The workflow has been completed successfully:</p>
                    <ul>
                        <li><strong>Workflow:</strong> {WorkflowName}</li>
                        <li><strong>Entity:</strong> {EntityType} #{EntityId}</li>
                        <li><strong>Completed:</strong> {DueDate}</li>
                    </ul>
                    <p>Thank you,<br/>ERP System</p>
                </body>
                </html>",

            ["WorkflowRejected"] = @"
                <html>
                <body>
                    <h2>Workflow Rejected: {WorkflowName}</h2>
                    <p>Hello {RecipientName},</p>
                    <p>The workflow was rejected:</p>
                    <ul>
                        <li><strong>Workflow:</strong> {WorkflowName}</li>
                        <li><strong>Entity:</strong> {EntityType} #{EntityId}</li>
                        <li><strong>Rejected At:</strong> {StepName}</li>
                        <li><strong>Comments:</strong> {Comments}</li>
                    </ul>
                    <p>Please review and resubmit if necessary.</p>
                    <p>Thank you,<br/>ERP System</p>
                </body>
                </html>"
        };

        await Task.CompletedTask;
        return templates.TryGetValue(templateName, out var template) ? template : string.Empty;
    }

    private string RenderTemplate<T>(string template, T model) where T : class
    {
        // Simple template rendering - replace placeholders with model properties
        var result = template;
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(model)?.ToString() ?? string.Empty;
            result = result.Replace($"{{{prop.Name}}}", value);
        }

        return result;
    }

    private string ExtractSubject<T>(string template, T model) where T : class
    {
        // Extract subject from first h2 tag or use default
        var match = System.Text.RegularExpressions.Regex.Match(template, @"<h2>(.*?)</h2>");
        if (match.Success)
        {
            var subject = match.Groups[1].Value;
            return RenderTemplate(subject, model);
        }

        return "ERP System Notification";
    }
}
