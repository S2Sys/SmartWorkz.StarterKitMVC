namespace SmartWorkz.Core;

/// <summary>
/// Service for sending SMS (Short Message Service) text messages to phone numbers.
/// </summary>
/// <remarks>
/// Purpose: Provides SMS delivery capabilities for sending text messages to mobile devices.
/// Common use cases: OTP/2FA codes, account confirmations, alerts, and reminders.
///
/// Error Handling: Uses Task-based methods (no return values; failures should be handled
/// via exception handling). Expected failures (invalid phone number, carrier rejection)
/// may throw exceptions. Callers should wrap SendAsync in try-catch and log failures.
///
/// Async Behavior: All methods are Task-based and await provider acknowledgment.
/// Important: Acknowledgment ≠ Delivery. The SMS is accepted by the SMS gateway/carrier
/// but may fail to reach the recipient (invalid number, phone offline, carrier failure, etc.).
///
/// Delivery Guarantee: Best-effort delivery. SMS is more reliable than push notifications
/// but not guaranteed. Carriers may reject messages for abuse prevention.
///
/// Rate Limiting: Most SMS providers enforce rate limits (e.g., 5 SMS per second per account).
/// Implement rate limiting to prevent abuse and unexpected billing.
///
/// Costs: SMS is typically paid per message. Budget and monitor usage to manage costs.
/// OTP/verification codes are often cheaper than promotional SMS.
///
/// Typical Usage: Injected into services for OTP delivery, account verification, alerts,
/// and critical notifications. Often used in background jobs for non-critical SMS.
///
/// Security:
/// - Validate phone numbers before sending
/// - Implement rate limiting per number (e.g., max 5 OTP attempts per hour)
/// - Do not log sensitive message content
/// - Use approved sender ID/number
/// - Implement opt-in/opt-out (unsubscribe) functionality for compliance
///
/// Compliance: SMS is regulated in many jurisdictions (GDPR, TCPA, CASL).
/// Implement consent management and respecting opt-out requests.
///
/// Performance: SMS sending is I/O-intensive. For non-critical messages, use background jobs.
/// </remarks>
public interface ISmsService : IService
{
    /// <summary>
    /// Sends an SMS message to a single phone number asynchronously.
    /// </summary>
    /// <param name="phoneNumber">The recipient phone number in E.164 format (e.g., "+12125551234").
    /// Must include country code. Other formats may not work correctly.</param>
    /// <param name="message">The SMS message content. Must not be null or empty.
    /// Typically limited to 160 characters (1 SMS) or 153 characters per part for longer messages.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// E.164 Format: Phone numbers must include the country code and plus sign.
    /// Examples:
    /// - "+12125551234" (USA)
    /// - "+441632960000" (UK)
    /// - "+33123456789" (France)
    /// - "+49307659332" (Germany)
    ///
    /// Message Length:
    /// - Single SMS: Up to 160 characters (ASCII) or 70 characters (Unicode/emoji)
    /// - Long SMS: Messages longer than this are split into multiple parts
    /// - Longer messages incur additional charges
    ///
    /// Validation: Should validate phone number format before sending.
    /// Invalid formats may throw ArgumentException.
    ///
    /// Error Codes (if exceptions are thrown):
    /// - SMS_INVALID_PHONE: Invalid phone number format
    /// - SMS_INVALID_MESSAGE: Message content invalid (empty, too long, etc.)
    /// - GATEWAY_ERROR: SMS gateway/provider error
    /// - RATE_LIMIT_EXCEEDED: Too many SMS sent in short time
    /// - NETWORK_ERROR: Network/connectivity failure
    /// - INVALID_NUMBER: Number is unreachable or invalid
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     await smsService.SendAsync(
    ///         phoneNumber: "+12125551234",
    ///         message: "Your verification code is: 123456"
    ///     );
    ///     logger.LogInformation($"SMS sent to {phoneNumber}");
    /// }
    /// catch (ArgumentException ex)
    /// {
    ///     logger.LogError($"Invalid phone number: {ex.Message}");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Failed to send SMS: {ex.Message}");
    /// }
    /// </code>
    /// </example>
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an SMS message to multiple phone numbers asynchronously.
    /// </summary>
    /// <param name="phoneNumbers">Collection of recipient phone numbers in E.164 format
    /// (e.g., "+12125551234"). Each must include country code. Must not be null or empty.</param>
    /// <param name="message">The SMS message content. Must not be null or empty.
    /// Same length limitations apply as single SMS send.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task representing the async send operation. Throws on failure.</returns>
    /// <remarks>
    /// Batch Sending: Sends the same message to all phone numbers.
    /// For personalized messages, call SendAsync multiple times.
    ///
    /// Implementation Options:
    /// 1. Send individual SMS to each number (slower but reliable)
    /// 2. Use SMS gateway batch API (more efficient)
    /// 3. Queue SMS and send asynchronously via background job
    ///
    /// Partial Failures: If one number fails, the entire operation may fail.
    /// Consult implementation docs for partial success handling.
    ///
    /// Rate Limiting: Sending to many numbers may hit rate limits.
    /// For large batches (&gt;100 numbers), consider using background jobs
    /// with staggered sending to avoid rate limit errors.
    ///
    /// Cost: Charged per message sent. A batch to 100 numbers costs 100x the single message price.
    /// Monitor usage to manage SMS costs.
    /// </remarks>
    /// <example>
    /// <code>
    /// var recipients = new[]
    /// {
    ///     "+12125551001",
    ///     "+12125551002",
    ///     "+12125551003"
    /// };
    ///
    /// try
    /// {
    ///     await smsService.SendAsync(
    ///         phoneNumbers: recipients,
    ///         message: "Account verification code: 654321"
    ///     );
    ///     logger.LogInformation($"SMS sent to {recipients.Length} recipients");
    /// }
    /// catch (Exception ex)
    /// {
    ///     logger.LogError($"Batch SMS send failed: {ex.Message}");
    ///     // Consider retry logic or fallback (email, push notification)
    /// }
    /// </code>
    /// </example>
    Task SendAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken cancellationToken = default);
}
