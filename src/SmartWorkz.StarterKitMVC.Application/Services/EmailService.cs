using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Abstractions;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Implementation of email service.
/// Sends emails via SMTP with retry logic and template support.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ISmtpService _smtpService;
    private readonly IEmailQueueRepository? _emailQueueRepository;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        ISmtpService smtpService,
        ILogger<EmailService> logger,
        IEmailQueueRepository? emailQueueRepository = null)
    {
        _smtpService = smtpService ?? throw new ArgumentNullException(nameof(smtpService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailQueueRepository = emailQueueRepository;
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
    {
        return await SendEmailAsync(new EmailMessage
        {
            To = toEmail,
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        });
    }

    /// <inheritdoc />
    public async Task<bool> SendEmailAsync(EmailMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        if (string.IsNullOrWhiteSpace(message.To))
            throw new ArgumentException("Recipient email is required", nameof(message.To));
        if (string.IsNullOrWhiteSpace(message.Subject))
            throw new ArgumentException("Email subject is required", nameof(message.Subject));

        try
        {
            var result = await _smtpService.SendAsync(message.To, message.Subject, message.Body, message.IsHtml);

            if (!result.Succeeded)
            {
                _logger.LogWarning(
                    "Failed to send email to {To}: {Error}",
                    message.To, string.Join(", ", result.Errors));
                return false;
            }

            _logger.LogInformation(
                "Email sent successfully to {To}: {Subject}",
                message.To, message.Subject);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending email to {To}: {Subject}",
                message.To, message.Subject);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body)
    {
        if (recipients == null || !recipients.Any())
            throw new ArgumentException("Recipients list cannot be empty", nameof(recipients));
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required", nameof(subject));

        try
        {
            var tasks = recipients.Select(recipient =>
                SendEmailAsync(recipient, subject, body, isHtml: true)
            );

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r);

            _logger.LogInformation(
                "Bulk email sent to {Total} recipients, {Success} successful: {Subject}",
                recipients.Count(), successCount, subject);

            return successCount == recipients.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk email");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendTemplateEmailAsync(string toEmail, string templateName, Dictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email is required", nameof(toEmail));
        if (string.IsNullOrWhiteSpace(templateName))
            throw new ArgumentException("Template name is required", nameof(templateName));

        try
        {
            // In a real implementation, you would load the template and replace variables
            var templateContent = await LoadTemplateAsync(templateName);
            var body = ReplaceVariables(templateContent, variables ?? new Dictionary<string, string>());

            return await SendEmailAsync(toEmail, templateName, body, isHtml: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending template email to {To}: {TemplateName}",
                toEmail, templateName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> QueueEmailAsync(EmailMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        if (_emailQueueRepository == null)
        {
            _logger.LogWarning("Email queue repository not configured, sending directly");
            return await SendEmailAsync(message);
        }

        try
        {
            var queueItem = new Domain.Entities.Shared.EmailQueue
            {
                ToEmail = message.To,
                CcEmail = message.Cc,
                BccEmail = message.Bcc,
                Subject = message.Subject,
                Body = message.Body,
                IsHtml = message.IsHtml,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                SendAttempts = 0
            };

            await _emailQueueRepository.EnqueueAsync(queueItem);

            _logger.LogDebug("Email queued for {To}: {Subject}", message.To, message.Subject);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing email for {To}", message.To);
            return false;
        }
    }

    /// <summary>
    /// Loads an email template by name.
    /// In a real implementation, this would read from the file system or database.
    /// </summary>
    private async Task<string> LoadTemplateAsync(string templateName)
    {
        // Placeholder implementation
        await Task.Delay(0);
        return $"<p>Template: {templateName}</p>";
    }

    /// <summary>
    /// Replaces variables in template content.
    /// Variables are marked with {{VariableName}}.
    /// </summary>
    private static string ReplaceVariables(string content, Dictionary<string, string> variables)
    {
        var result = content;
        foreach (var variable in variables)
        {
            result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value ?? string.Empty);
        }
        return result;
    }
}

/// <summary>DTO for Email Queue entity</summary>
public class EmailQueueDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string To { get; set; }
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string Status { get; set; } // Pending, Sent, Failed
    public int Attempts { get; set; }
    public int MaxAttempts { get; set; }
    public string? ErrorMessage { get; set; }
}
