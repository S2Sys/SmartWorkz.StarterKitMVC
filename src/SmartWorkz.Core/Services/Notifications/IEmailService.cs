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
/// Data structure for complex email transmission via IEmailService with support for multiple recipients and attachments.
/// </summary>
/// <remarks>
/// Purpose: Encapsulates all email message data (recipients, content, attachments, formatting) for IEmailService.SendAsync.
///
/// Service Consumer: IEmailService.SendAsync(EmailMessage) validates and transmits the message via configured email provider
/// (SMTP, SendGrid, AWS SES, etc.).
///
/// Usage Pattern:
/// 1. Create new EmailMessage instance
/// 2. Set required properties: To, Subject, Body
/// 3. Optionally set: Cc, Bcc, IsHtml, Attachments
/// 4. Pass to emailService.SendAsync(message, cancellationToken)
/// 5. Wrap in try-catch to handle validation and transmission errors
///
/// Validation Constraints:
/// - To: Must be valid RFC 5322 email address; cannot be null or whitespace
/// - Subject: Must not be null or empty; recommended &lt;200 characters for display in email clients
/// - Body: Must not be null or empty; content type determined by IsHtml property
/// - Cc/Bcc: Must be valid email addresses if provided; null or empty string acceptable
/// - Attachments: Optional; each file must be within provider limits (typically 10-25 MB per file, 25-50 MB total)
///
/// Nullable Fields:
/// - Cc, Bcc, Attachments are nullable and safely default to null
/// - IsHtml defaults to false (plain text); set true for HTML content
///
/// Security Considerations:
/// - Never log message body or recipient addresses to avoid data leakage
/// - Validate attachment content before sending (no malware, no excessively large files)
/// - Use TLS/SSL for email transmission in production
/// - Implement rate limiting (e.g., max 100 emails per minute) to prevent spam/abuse
/// - Consider DKIM, SPF, DMARC records for domain authentication to improve deliverability
///
/// Error Handling: IEmailService.SendAsync may throw:
/// - ArgumentException: Invalid email format in To/Cc/Bcc
/// - InvalidOperationException: Attachment too large or total size exceeds limit
/// - SmtpException or similar: Network/provider errors (wrap in try-catch)
/// </remarks>
public class EmailMessage
{
    /// <summary>
    /// Gets or sets the primary recipient email address (required).
    /// </summary>
    /// <remarks>
    /// Format: RFC 5322 compliant email address (e.g., "user@example.com", "john.doe+tag@example.co.uk").
    /// Validation: IEmailService.SendAsync validates format before transmission; invalid format throws ArgumentException.
    /// Required: Must not be null, empty, or whitespace.
    /// Recipient Type: Single primary recipient; use Cc for secondary/visibility recipients, Bcc for hidden copies.
    ///
    /// Validation Rules:
    /// - Must contain exactly one @ symbol
    /// - Domain part must be valid (contains dot, minimum 2 characters after dot)
    /// - No leading/trailing whitespace
    /// - Length typically &lt;254 characters (RFC 5321)
    ///
    /// Examples of Valid Values:
    /// - "user@example.com"
    /// - "john.doe@company.co.uk"
    /// - "support+ticket123@domain.org"
    /// - "firstname.lastname@subdomain.example.com"
    /// </remarks>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets comma-separated carbon copy (CC) recipient email addresses (optional).
    /// </summary>
    /// <remarks>
    /// Format: Comma-separated list of RFC 5322 email addresses (e.g., "manager@example.com,supervisor@example.com").
    /// Purpose: CC recipients can see all other recipients and are visible in the email headers.
    /// Nullable: Yes, null or empty string means no CC recipients.
    /// Typical Usage: Send to primary recipient and carbon copy to manager/supervisor for visibility.
    ///
    /// Validation Rules:
    /// - Each address must be valid email format (same rules as To field)
    /// - Addresses separated by commas (with or without spaces)
    /// - IEmailService.SendAsync validates each address and throws ArgumentException if any invalid
    ///
    /// Examples of Valid Values:
    /// - "manager@example.com,supervisor@example.com"
    /// - "john@example.com, jane@example.com, admin@company.com"
    /// - null (no CC recipients)
    /// - "" (empty string, no CC recipients)
    /// </remarks>
    public string? Cc { get; set; }

    /// <summary>
    /// Gets or sets comma-separated blind carbon copy (BCC) recipient email addresses (optional).
    /// </summary>
    /// <remarks>
    /// Format: Comma-separated list of RFC 5322 email addresses.
    /// Purpose: BCC recipients receive the email but are hidden from all other recipients; not visible in headers.
    /// Nullable: Yes, null or empty string means no BCC recipients.
    /// Typical Usage: Administrative/audit copies (e.g., send transaction to customer, BCC to company audit account).
    /// Privacy: Other recipients cannot see BCC list; useful for compliance and record-keeping.
    ///
    /// Validation Rules:
    /// - Each address must be valid email format (same rules as To field)
    /// - Addresses separated by commas (with or without spaces)
    ///
    /// Examples of Valid Values:
    /// - "audit@company.com"
    /// - "compliance@company.com,legal@company.com"
    /// - null (no BCC recipients)
    /// - "" (empty string, no BCC recipients)
    /// </remarks>
    public string? Bcc { get; set; }

    /// <summary>
    /// Gets or sets the email subject line (required).
    /// </summary>
    /// <remarks>
    /// Format: Plain text subject line (newlines typically not allowed; single line).
    /// Length: Recommended &lt;200 characters maximum; most email clients display 50-80 characters in preview.
    /// Validation: Must not be null, empty, or whitespace only.
    /// Best Practices:
    /// - Use descriptive, action-oriented subject lines
    /// - Avoid generic subjects ("Message" or "Notification")
    /// - Include transaction/order IDs when relevant (e.g., "Order Confirmation #12345")
    /// - Use title case or sentence case for readability
    /// - Avoid excessive capitalization or spam trigger words
    ///
    /// Examples of Valid Values:
    /// - "Password Reset Request"
    /// - "Order Confirmation #12345 - Thank You"
    /// - "Invoice #INV-2024-001 Attached"
    /// - "Welcome to SmartWorkz - Account Activated"
    /// - "System Maintenance Scheduled for Tonight at 10 PM"
    /// </remarks>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email body content (required).
    /// </summary>
    /// <remarks>
    /// Format: Plain text or HTML depending on IsHtml property.
    /// - If IsHtml=false (default): Plain text body; simple text content
    /// - If IsHtml=true: HTML body; should contain valid, well-formed HTML
    ///
    /// Validation: Must not be null, empty, or whitespace only.
    /// Length: No hard limit, but providers may have practical limits (e.g., 2-5 MB).
    /// Encoding: Automatically encoded by IEmailService.SendAsync for transmission.
    ///
    /// Security & Best Practices:
    /// - Never include passwords, API keys, or sensitive tokens in plain text
    /// - Use secure links with expiring tokens for password resets or account actions
    /// - Sanitize any user-generated content to prevent HTML injection
    /// - Keep HTML simple and email-client compatible (many clients restrict CSS/JavaScript)
    /// - Provide text/plain fallback if sending HTML
    /// - Test in multiple email clients for rendering issues
    ///
    /// HTML Best Practices (when IsHtml=true):
    /// - Use inline CSS (style attributes) instead of style tags
    /// - Avoid JavaScript entirely (email clients strip scripts)
    /// - Use standard HTML tags; test with email validation tools
    /// - Provide alt text for images
    /// - Use absolute URLs (not relative) for links and images
    /// - Respect dark mode in email clients (test appearance)
    ///
    /// Examples of Valid Values:
    /// - Plain text: "Your verification code is: 123456. This code expires in 10 minutes."
    /// - HTML: "&lt;h1&gt;Welcome!&lt;/h1&gt;&lt;p&gt;Thank you for joining SmartWorkz.&lt;/p&gt;&lt;a href=...&gt;Confirm Email&lt;/a&gt;"
    /// </remarks>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the body should be sent as HTML content (optional, defaults to false).
    /// </summary>
    /// <remarks>
    /// Default: false (plain text)
    /// Behavior:
    /// - true: Body is sent with Content-Type: text/html; charset=utf-8 (HTML rendering in email clients)
    /// - false: Body is sent with Content-Type: text/plain; charset=utf-8 (plain text display)
    ///
    /// When to Use HTML (IsHtml=true):
    /// - Professional emails with formatted layouts (newsletters, invoices, marketing)
    /// - Emails requiring images, colors, or rich formatting
    /// - Transactional emails with styled sections (order confirmations, receipts)
    /// - When you want links with custom styling or button appearances
    ///
    /// When to Use Plain Text (IsHtml=false):
    /// - Simple notifications or alerts
    /// - Sensitive communications (security alerts, compliance notices)
    /// - When HTML support is uncertain or not needed
    /// - Better for email deliverability (some spam filters distrust heavy HTML)
    ///
    /// HTML Validation: IEmailService.SendAsync does NOT validate HTML structure.
    /// - Your application must ensure Body contains valid, well-formed HTML when IsHtml=true
    /// - Malformed HTML may render poorly in email clients
    /// - Use email HTML validation tools before setting IsHtml=true
    ///
    /// Email Client Support:
    /// - Most modern email clients (Gmail, Outlook, Apple Mail) support HTML
    /// - Some enterprise/legacy clients may restrict HTML features (JavaScript, external stylesheets)
    /// - Always test in target email clients before sending high-volume campaigns
    /// </remarks>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets file attachments as a dictionary of filename to binary content (optional).
    /// </summary>
    /// <remarks>
    /// Format: Dictionary&lt;string, byte[]&gt; where:
    /// - Key: Filename with extension (e.g., "invoice.pdf", "report_2024.xlsx")
    /// - Value: Binary file content (bytes)
    /// - Nullable: Yes; null means no attachments
    ///
    /// Typical Provider Size Limits:
    /// - Single attachment: 10-25 MB (varies by provider)
    /// - Total per email: 25-50 MB (varies by provider)
    /// - Number of attachments: 5-20 (varies by provider)
    /// - Examples: Gmail 25 MB total, Outlook 20 MB total, SendGrid 30 MB total
    /// - Consult your email provider's documentation for exact limits
    ///
    /// Validation: IEmailService.SendAsync may throw InvalidOperationException if:
    /// - Single attachment exceeds provider limit
    /// - Total attachment size exceeds provider limit
    /// - Number of attachments exceeds provider limit
    ///
    /// Security Considerations:
    /// - Validate attachment content before adding (no malware, no excessively large files)
    /// - Scan uploads with antivirus if attaching user-generated files
    /// - Log attachment attempt but NOT attachment content
    /// - Consider virus scanning for user uploads before emailing
    /// - Be cautious with executable files (.exe, .bat, .scr); many email providers block these
    /// - Use secure file transfer for sensitive files instead of email attachments
    ///
    /// Supported File Types:
    /// - Documents: .pdf, .doc, .docx, .xls, .xlsx, .txt, .rtf
    /// - Images: .jpg, .jpeg, .png, .gif, .bmp (inline or attached)
    /// - Archives: .zip, .rar (may be blocked by some providers)
    /// - Executable/Code: .exe, .bat, .com, .scr, .dll (typically blocked by providers)
    /// - Other: Consult provider's blocked file list
    ///
    /// Usage Pattern:
    /// new Dictionary&lt;string, byte[]&gt;
    /// {
    ///     { "invoice_12345.pdf", invoicePdfBytes },
    ///     { "payment_receipt.txt", receiptBytes },
    ///     { "terms_and_conditions.pdf", termsBytes }
    /// }
    ///
    /// Examples of Valid Values:
    /// - Dictionary with one PDF: { "invoice.pdf", pdfContent }
    /// - Dictionary with multiple files: { "report.pdf", reportBytes }, { "data.csv", csvBytes }
    /// - null (no attachments)
    /// - Empty Dictionary (no attachments)
    /// </remarks>
    public Dictionary<string, byte[]>? Attachments { get; set; }

    /// <example>
    /// Simple text email to single recipient:
    /// <code>
    /// var welcomeEmail = new EmailMessage
    /// {
    ///     To = "newuser@example.com",
    ///     Subject = "Welcome to SmartWorkz",
    ///     Body = "Welcome to SmartWorkz! Your account has been activated. You can now log in.",
    ///     IsHtml = false
    /// };
    /// await emailService.SendAsync(welcomeEmail);
    /// </code>
    ///
    /// HTML email with CC to manager:
    /// <code>
    /// var reportEmail = new EmailMessage
    /// {
    ///     To = "team@example.com",
    ///     Cc = "manager@example.com",
    ///     Subject = "Monthly Sales Report - March 2024",
    ///     Body = @"&lt;h2&gt;Sales Summary&lt;/h2&gt;
    ///              &lt;p&gt;Total Revenue: $50,000&lt;/p&gt;
    ///              &lt;p&gt;Total Orders: 150&lt;/p&gt;
    ///              &lt;a href='https://reports.example.com/march-2024'&gt;View Detailed Report&lt;/a&gt;",
    ///     IsHtml = true
    /// };
    /// await emailService.SendAsync(reportEmail);
    /// </code>
    ///
    /// Complex email with attachments and BCC:
    /// <code>
    /// var invoiceEmail = new EmailMessage
    /// {
    ///     To = "customer@example.com",
    ///     Cc = "accounting@company.com",
    ///     Bcc = "audit@company.com",
    ///     Subject = "Invoice #INV-2024-12345",
    ///     Body = "&lt;h1&gt;Invoice&lt;/h1&gt;&lt;p&gt;Please find your invoice attached.&lt;/p&gt;",
    ///     IsHtml = true,
    ///     Attachments = new Dictionary&lt;string, byte[]&gt;
    ///     {
    ///         { "invoice_2024_12345.pdf", invoicePdfBytes },
    ///         { "payment_terms.pdf", termsBytes }
    ///     }
    /// };
    /// try
    /// {
    ///     await emailService.SendAsync(invoiceEmail);
    ///     logger.LogInformation($"Invoice email sent to {invoiceEmail.To}");
    /// }
    /// catch (ArgumentException ex)
    /// {
    ///     logger.LogError($"Invalid email address: {ex.Message}");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Failed to send invoice email: {ex.Message}");
    ///     // Consider retry logic or fallback notification
    /// }
    /// </code>
    ///
    /// Password reset email with secure token:
    /// <code>
    /// var resetToken = await tokenService.GeneratePasswordResetTokenAsync(user);
    /// var resetUrl = $"https://app.example.com/reset-password?token={resetToken}";
    ///
    /// var resetEmail = new EmailMessage
    /// {
    ///     To = user.Email,
    ///     Subject = "Reset Your Password",
    ///     Body = $@"&lt;p&gt;You requested a password reset. This link expires in 24 hours.&lt;/p&gt;
    ///              &lt;a href='{resetUrl}' style='background-color: blue; color: white; padding: 10px;'&gt;
    ///                Reset Password
    ///              &lt;/a&gt;
    ///              &lt;p&gt;If you didn't request this, ignore this email.&lt;/p&gt;",
    ///     IsHtml = true
    /// };
    /// await emailService.SendAsync(resetEmail);
    /// </code>
    /// </example>
}
