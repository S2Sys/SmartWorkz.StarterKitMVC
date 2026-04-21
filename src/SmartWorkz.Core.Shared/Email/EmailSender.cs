namespace SmartWorkz.Shared;

using System.Net.Mail;
using System.Net;

public sealed class EmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;

    public EmailSender(SmtpSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<Result> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            using var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(_settings.FromAddress, _settings.FromDisplayName);
            mailMessage.Subject = message.Subject;
            mailMessage.Body = message.Body;
            mailMessage.IsBodyHtml = message.IsHtml;

            foreach (var recipient in message.To)
                mailMessage.To.Add(new MailAddress(recipient));

            if (message.Cc?.Count > 0)
                foreach (var cc in message.Cc)
                    mailMessage.CC.Add(new MailAddress(cc));

            if (message.Bcc?.Count > 0)
                foreach (var bcc in message.Bcc)
                    mailMessage.Bcc.Add(new MailAddress(bcc));

            if (!string.IsNullOrEmpty(message.ReplyTo))
                mailMessage.ReplyToList.Add(new MailAddress(message.ReplyTo));

            if (message.Attachments?.Count > 0)
            {
                foreach (var attachment in message.Attachments)
                {
                    var memStream = new MemoryStream(attachment.Content);
                    mailMessage.Attachments.Add(new Attachment(memStream, attachment.FileName,
                        attachment.ContentType ?? "application/octet-stream"));
                }
            }

            using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                Timeout = _settings.TimeoutMs
            };

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error("EMAIL.SEND_FAILED", ex.Message));
        }
    }

    public async Task<Result> SendAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage { To = [to], Subject = subject, Body = body, IsHtml = isHtml };
        return await SendAsync(message, cancellationToken);
    }

    public async Task<Result> SendAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage { To = recipients.ToList(), Subject = subject, Body = body, IsHtml = isHtml };
        return await SendAsync(message, cancellationToken);
    }
}

public sealed class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int TimeoutMs { get; set; } = 10000;
}
