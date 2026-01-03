namespace SmartWorkz.StarterKitMVC.Application.EmailTemplates;

/// <summary>
/// Service interface for sending emails using templates.
/// Integrates email templates with the notification system.
/// </summary>
/// <example>
/// <code>
/// // Send a welcome email using a template
/// await emailSender.SendTemplatedEmailAsync(
///     templateId: "welcome-email",
///     recipient: "user@example.com",
///     data: new Dictionary&lt;string, object&gt;
///     {
///         ["UserName"] = "John Doe",
///         ["LoginUrl"] = "https://example.com/login"
///     }
/// );
/// </code>
/// </example>
public interface ITemplatedEmailSender
{
    /// <summary>
    /// Sends an email using a template.
    /// </summary>
    /// <param name="templateId">The template ID to use.</param>
    /// <param name="recipient">The recipient email address.</param>
    /// <param name="data">Dictionary of placeholder values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the email was sent successfully.</returns>
    Task<bool> SendTemplatedEmailAsync(
        string templateId,
        string recipient,
        IDictionary<string, object> data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email using a template to multiple recipients.
    /// </summary>
    /// <param name="templateId">The template ID to use.</param>
    /// <param name="recipients">The recipient email addresses.</param>
    /// <param name="data">Dictionary of placeholder values (shared for all recipients).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of emails sent successfully.</returns>
    Task<int> SendTemplatedEmailAsync(
        string templateId,
        IEnumerable<string> recipients,
        IDictionary<string, object> data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a personalized email to multiple recipients using a template.
    /// </summary>
    /// <param name="templateId">The template ID to use.</param>
    /// <param name="recipientData">Dictionary mapping recipient email to their placeholder data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of emails sent successfully.</returns>
    Task<int> SendPersonalizedEmailsAsync(
        string templateId,
        IDictionary<string, IDictionary<string, object>> recipientData,
        CancellationToken cancellationToken = default);
}
