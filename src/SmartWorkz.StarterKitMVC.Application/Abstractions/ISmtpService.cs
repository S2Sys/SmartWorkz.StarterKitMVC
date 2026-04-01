using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

/// <summary>
/// SMTP email delivery service abstraction.
/// Implementations handle actual SMTP client connection and message sending.
/// Deferred implementation — stub interface for future use.
/// </summary>
public interface ISmtpService
{
    /// <summary>
    /// Sends an email via SMTP.
    /// </summary>
    Task<Result> SendAsync(string toEmail, string subject, string body, bool isHtml = true, CancellationToken ct = default);

    /// <summary>
    /// Sends an email with CC/BCC recipients.
    /// </summary>
    Task<Result> SendAsync(string toEmail, string? ccEmail, string? bccEmail,
        string subject, string body, bool isHtml = true, CancellationToken ct = default);
}
