namespace SmartWorkz.Core;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);
    Task SendAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);
    Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public Dictionary<string, byte[]>? Attachments { get; set; }
}
