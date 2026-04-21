namespace SmartWorkz.Shared;

public interface IEmailSender
{
    Task<Result> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    Task<Result> SendAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);
    Task<Result> SendAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);
}

public sealed class EmailMessage
{
    public string From { get; set; } = string.Empty;
    public List<string> To { get; set; } = [];
    public List<string> Cc { get; set; } = [];
    public List<string> Bcc { get; set; } = [];
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
    public string? ReplyTo { get; set; }
}

public sealed class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
    public string? ContentType { get; set; }
}
