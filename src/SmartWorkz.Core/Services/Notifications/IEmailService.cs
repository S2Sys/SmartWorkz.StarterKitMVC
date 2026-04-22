namespace SmartWorkz.Core;

/// <summary>
/// Service for sending email messages with support for multiple recipients, attachments, and formatting.
/// </summary>
/// <remarks>
/// Purpose: Provides email sending capabilities with support for simple strings, batch sends,
/// and complex messages with attachments.
///
/// Error Handling: Uses Task-based methods (no return values; failures should be handled
/// via exception handling or logging). Expected failures (invalid recipients, SMTP timeouts)
/// may throw exceptions. Callers should wrap SendAsync in try-catch and log failures.
///
/// Async Behavior: All methods are Task-based and await provider acknowledgment.
/// Important: Acknowledgment != Delivery. The email is accepted by SMTP/provider but
/// may still fail to reach the recipient (bounce, spam filter, etc.).
///
/// Retry Logic: Implementations may include automatic retry on transient failures
/// (connection timeouts, service unavailable). Consult implementation docs.
///
/// Typical Usage: Injected into services that need to send notifications, confirmations,
/// password resets, etc. Often used in request handlers after database operations.
///
/// Security:
/// - Validate recipient email addresses before sending
/// - Do not log email bodies or sensitive content
/// - Use TLS/SSL for SMTP connections
/// - Implement rate limiting to prevent spam/abuse
/// - Consider adding DKIM, SPF, DMARC records for domain authentication
///
/// Performance: Email sending is I/O-intensive. Use background jobs (Hangfire, etc.)
/// for non-critical emails to avoid blocking request handling.
/// </remarks>
public interface IEmailService : IService
{
    /// <summary>
    /// Sends an email to a single recipient asynchronously.
    /// </summary>
    /// <param name="to">The recipient email address. Must be a valid email format.</param>
    /// <param name="subject">The email subject line. Must not be null or empty.</param>
    /// <param name="body">The email body content. Must not be null or empty.</param>
    /// <param name="isHtml">If true, body is treated as HTML and sent with text/html content type.
    /// If false, body is sent as plain text. Defaults to false.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// Email Validation: Should validate the recipient address format before sending.
    /// Invalid formats may throw ArgumentException or similar.
    ///
    /// Content Type: Setting isHtml=true sends text/html content type. Ensure the body
    /// contains valid HTML to avoid rendering issues in email clients.
    ///
    /// Async Behavior: Awaits provider acknowledgment (email accepted by SMTP), not delivery.
    /// Email may still fail to reach the recipient after acknowledgment.
    ///
    /// Error Codes (if exceptions are thrown):
    /// - EMAIL_INVALID_RECIPIENT: Invalid email address format
    /// - SMTP_FAILURE: SMTP server error
    /// - RATE_LIMIT_EXCEEDED: Too many emails sent in short time
    /// - TIMEOUT: SMTP operation timed out
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await emailService.SendAsync(
    ///         to: "user@example.com",
    ///         subject: "Welcome to SmartWorkz",
    ///         body: "Welcome to our application!",
    ///         isHtml: false
    ///     );
    ///     logger.LogInformation("Welcome email sent to user@example.com");
    /// }
    /// catch (ArgumentException ex)
    /// {
    ///     logger.LogError($"Invalid email address: {ex.Message}");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Email send failed: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SendAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email to multiple recipients asynchronously.
    /// </summary>
    /// <param name="recipients">Collection of recipient email addresses. Each must be a valid email format.
    /// Must not be null or empty.</param>
    /// <param name="subject">The email subject line. Must not be null or empty.</param>
    /// <param name="body">The email body content. Must not be null or empty.</param>
    /// <param name="isHtml">If true, body is treated as HTML. If false, body is plain text. Defaults to false.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// Batch Sending: Sends the same email to all recipients. For personalized emails,
    /// call SendAsync multiple times or use the EmailMessage overload.
    ///
    /// Implementation Options:
    /// 1. Send separate email to each recipient
    /// 2. Use BCC to send one email with all recipients hidden
    /// 3. Use SMTP mail merge if supported by provider
    ///
    /// Partial Failures: If one recipient fails, the entire operation may fail. Consult
    /// implementation docs for partial success handling.
    ///
    /// Performance: Batch sending to many recipients may take longer. For large lists,
    /// consider using background jobs or the provider's bulk email API.
    /// </remarks>
    /// <example>
    /// <code>
    /// var recipients = new[] { "user1@example.com", "user2@example.com", "user3@example.com" };
    /// try
    /// {
    ///     await emailService.SendAsync(
    ///         recipients: recipients,
    ///         subject: "System Maintenance Notice",
    ///         body: "The system will be under maintenance tomorrow at 10 PM.",
    ///         isHtml: false
    ///     );
    ///     logger.LogInformation($"Maintenance notice sent to {recipients.Length} users");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Batch email send failed: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SendAsync(IEnumerable<string> recipients, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a complex email message with support for CC, BCC, attachments, and custom formatting.
    /// </summary>
    /// <param name="emailMessage">The email message containing To, Subject, Body, and optional Cc, Bcc, Attachments.
    /// Must not be null.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// Complex Messages: Use this overload for emails with:
    /// - Multiple recipients (To)
    /// - Carbon copy (CC) recipients who see all recipients
    /// - Blind carbon copy (BCC) recipients hidden from other recipients
    /// - File attachments (documents, images)
    ///
    /// Attachments: Provided as Dictionary&lt;string, byte[]&gt; where key is filename
    /// and value is binary content. Large attachments may be rejected by providers.
    /// Typical limits: 25-50 MB per email.
    ///
    /// Security Considerations:
    /// - Validate attachment content before sending (no malware, excessive size)
    /// - Scan attachments with antivirus if storing user uploads
    /// - Log attempt but not attachment content
    ///
    /// Async Behavior: Awaits provider acknowledgment. Large attachments may increase
    /// send time.
    ///
    /// Error Codes (if exceptions are thrown):
    /// - EMAIL_INVALID_RECIPIENT: Invalid email in To/Cc/Bcc
    /// - ATTACHMENT_TOO_LARGE: Total attachment size exceeds limit
    /// - SMTP_FAILURE: SMTP server error
    /// - TIMEOUT: SMTP operation timed out
    /// </remarks>
    /// <example>
    /// <code>
    /// var invoice = await invoiceService.GetByIdAsync(invoiceId);
    /// var invoicePdf = await pdfGenerator.GenerateAsync(invoice);
    ///
    /// var message = new EmailMessage
    /// {
    ///     To = "customer@example.com",
    ///     Cc = "accounting@company.com",
    ///     Subject = "Invoice #12345",
    ///     Body = "<h1>Invoice</h1><p>Please see attached invoice.</p>",
    ///     IsHtml = true,
    ///     Attachments = new Dictionary&lt;string, byte[]&gt;
    ///     {
    ///         { "invoice_12345.pdf", invoicePdf }
    ///     }
    /// };
    ///
    /// try
    /// {
    ///     await emailService.SendAsync(message);
    ///     logger.LogInformation("Invoice email sent with attachment");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Failed to send invoice email: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a complete email message with recipients, content, and attachments.
/// </summary>
/// <remarks>
/// This class is used by IEmailService.SendAsync to send complex emails with multiple
/// recipients, attachments, and formatting options.
///
/// Typical usage:
/// 1. Create new EmailMessage
/// 2. Set To, Subject, Body (required)
/// 3. Set Cc, Bcc, IsHtml, Attachments (optional)
/// 4. Pass to emailService.SendAsync
/// </remarks>
public class EmailMessage
{
    /// <summary>
    /// Gets or sets the primary recipient email address.
    /// </summary>
    /// <remarks>
    /// Must be a valid email format. Required, must not be null or empty.
    /// Example: "user@example.com"
    /// </remarks>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets comma-separated carbon copy (CC) recipient email addresses.
    /// </summary>
    /// <remarks>
    /// Optional. CC recipients can see all other recipients and are notified in the
    /// email header. Set to null or empty string if no CC recipients.
    /// Example: "manager@example.com,supervisor@example.com"
    /// </remarks>
    public string? Cc { get; set; }

    /// <summary>
    /// Gets or sets comma-separated blind carbon copy (BCC) recipient email addresses.
    /// </summary>
    /// <remarks>
    /// Optional. BCC recipients receive the email but are not visible to other recipients.
    /// Useful for administrative/audit copies. Set to null or empty string if no BCC.
    /// Example: "audit@company.com"
    /// </remarks>
    public string? Bcc { get; set; }

    /// <summary>
    /// Gets or sets the email subject line.
    /// </summary>
    /// <remarks>
    /// Required, must not be null or empty. Keep subject lines concise and descriptive.
    /// Example: "Password Reset Request" or "Order Confirmation #12345"
    /// </remarks>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email body content (plain text or HTML).
    /// </summary>
    /// <remarks>
    /// Required, must not be null or empty. Format depends on IsHtml:
    /// - If IsHtml=false: Plain text body
    /// - If IsHtml=true: HTML body (ensure valid HTML)
    /// </remarks>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the body should be sent as HTML.
    /// </summary>
    /// <remarks>
    /// true: Body is sent with text/html content type
    /// false: Body is sent as plain text (default)
    ///
    /// When true, ensure Body contains valid HTML. Most email clients support
    /// HTML but some may have restrictions for security.
    /// </remarks>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets file attachments as a dictionary of filename to binary content.
    /// </summary>
    /// <remarks>
    /// Optional. Dictionary key is the filename (e.g., "document.pdf"), value is the file content (bytes).
    /// Set to null if no attachments.
    ///
    /// Typical size limits:
    /// - Single attachment: 10-25 MB
    /// - Total attachments: 25-50 MB
    /// - Number of attachments: 10-20
    ///
    /// Consult your email provider's documentation for specific limits.
    ///
    /// Example:
    /// new Dictionary&lt;string, byte[]&gt;
    /// {
    ///     { "invoice.pdf", pdfContent },
    ///     { "receipt.txt", receiptContent }
    /// }
    /// </remarks>
    public Dictionary<string, byte[]>? Attachments { get; set; }
}
