using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

/// <summary>
/// SMS delivery service abstraction.
/// Implementations handle SMS provider integration (Twilio, AWS SNS, etc.).
/// Deferred implementation — stub interface for future use.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message to a phone number.
    /// </summary>
    Task<Result> SendAsync(string phoneNumber, string message, CancellationToken ct = default);

    /// <summary>
    /// Sends an SMS to multiple recipients.
    /// </summary>
    Task<Result> SendBatchAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken ct = default);
}
