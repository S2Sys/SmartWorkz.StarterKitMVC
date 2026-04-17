using System.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Repository for EmailQueue persistence.
/// Implements IEmailQueueRepository using Shared.sp_* stored procedures.
/// </summary>
public sealed class EmailQueueRepository : CachedDapperRepository, IEmailQueueRepository
{
    public EmailQueueRepository(IDbConnection connection, IMemoryCache cache, ILogger<EmailQueueRepository> logger)
        : base(connection, cache, logger)
    {
    }

    public async Task<int> EnqueueAsync(EmailQueue email, CancellationToken ct = default)
    {
        var emailQueueId = await QuerySingleSpAsync<int?>(
            "Shared.sp_EnqueueEmail",
            new
            {
                email.ToEmail,
                email.CcEmail,
                email.BccEmail,
                email.Subject,
                email.Body,
                email.IsHtml,
                email.TenantId,
                email.CreatedBy
            },
            timeoutSeconds: 10);

        return emailQueueId ?? 0;
    }

    public async Task<IEnumerable<EmailQueue>> GetPendingAsync(int batchSize = 50, int maxAttempts = 3, CancellationToken ct = default)
    {
        var emails = await QuerySpAsync<EmailQueue>(
            "Shared.sp_GetPendingEmails",
            new { BatchSize = batchSize, MaxAttempts = maxAttempts },
            timeoutSeconds: 10);

        return emails;
    }

    public async Task MarkSentAsync(int emailQueueId, CancellationToken ct = default)
    {
        await ExecuteSpAsync(
            "Shared.sp_MarkEmailSent",
            new { EmailQueueId = emailQueueId },
            timeoutSeconds: 10);
    }

    public async Task MarkFailedAsync(int emailQueueId, string? failureReason, CancellationToken ct = default)
    {
        await ExecuteSpAsync(
            "Shared.sp_MarkEmailFailed",
            new { EmailQueueId = emailQueueId, FailureReason = failureReason },
            timeoutSeconds: 10);
    }
}
