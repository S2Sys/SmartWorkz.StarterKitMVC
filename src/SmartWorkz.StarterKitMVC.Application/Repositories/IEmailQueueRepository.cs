using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Application.Repositories;

/// <summary>
/// Repository interface for persisting and tracking email delivery via the Shared.EmailQueue table.
/// Bridges template rendering and actual SMTP delivery.
/// </summary>
public interface IEmailQueueRepository
{
    /// <summary>
    /// Enqueues an email for delivery.
    /// Returns the auto-generated EmailQueueId.
    /// </summary>
    Task<int> EnqueueAsync(EmailQueue email, CancellationToken ct = default);

    /// <summary>
    /// Retrieves pending emails up to the batch size that haven't exceeded max retry attempts.
    /// Used by background delivery service to claim and send emails.
    /// </summary>
    Task<IEnumerable<EmailQueue>> GetPendingAsync(int batchSize = 50, int maxAttempts = 3, CancellationToken ct = default);

    /// <summary>
    /// Marks an email as successfully sent.
    /// Updates Status='Sent' and SentAt=now.
    /// </summary>
    Task MarkSentAsync(int emailQueueId, CancellationToken ct = default);

    /// <summary>
    /// Marks an email as failed after a send attempt.
    /// Updates SendAttempts counter. If attempts >= 3, Status='Failed'; else Status='Pending'.
    /// </summary>
    Task MarkFailedAsync(int emailQueueId, string? failureReason, CancellationToken ct = default);
}
