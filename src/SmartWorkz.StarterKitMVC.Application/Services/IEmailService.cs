namespace SmartWorkz.StarterKitMVC.Application.Services;

/// <summary>
/// Service for sending emails with template support.
/// Includes SMTP configuration, retry logic, and bulk email support.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to a single recipient.
    /// </summary>
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);

    /// <summary>
    /// Sends an email using an EmailMessage object.
    /// </summary>
    Task<bool> SendEmailAsync(EmailMessage message);

    /// <summary>
    /// Sends the same email to multiple recipients.
    /// </summary>
    Task<bool> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body);

    /// <summary>
    /// Sends an email using a template with variable substitution.
    /// </summary>
    Task<bool> SendTemplateEmailAsync(string toEmail, string templateName, Dictionary<string, string> variables);

    /// <summary>
    /// Queues an email for asynchronous sending.
    /// </summary>
    Task<bool> QueueEmailAsync(EmailMessage message);
}

/// <summary>Email message object</summary>
public class EmailMessage
{
    public string To { get; set; }
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public List<EmailAttachment> Attachments { get; set; } = new();
}

/// <summary>Email attachment object</summary>
public class EmailAttachment
{
    public string Filename { get; set; }
    public byte[] Content { get; set; }
    public string? ContentType { get; set; }
}
