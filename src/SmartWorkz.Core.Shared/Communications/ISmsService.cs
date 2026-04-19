namespace SmartWorkz.Core.Shared.Communications;

/// <summary>
/// Defines a contract for SMS communication services.
/// Provides methods for sending SMS messages to single or multiple recipients.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message to a single recipient.
    /// </summary>
    /// <param name="phoneNumber">The recipient phone number (E.164 format recommended)</param>
    /// <param name="message">The SMS message content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the SMS ID if successful</returns>
    Task<Result<string>> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an SMS message to multiple recipients (batch).
    /// </summary>
    /// <param name="phoneNumbers">Collection of recipient phone numbers</param>
    /// <param name="message">The SMS message content sent to all recipients</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing list of SMS IDs if successful</returns>
    Task<Result<IReadOnlyList<string>>> SendBatchAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken cancellationToken = default);
}
