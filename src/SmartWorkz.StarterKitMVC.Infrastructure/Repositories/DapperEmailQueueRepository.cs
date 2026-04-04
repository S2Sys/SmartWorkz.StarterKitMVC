using System.Data;
using Dapper;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for EmailQueue persistence.
/// Implements IEmailQueueRepository using Shared.sp_* stored procedures.
/// </summary>
public sealed class DapperEmailQueueRepository : IEmailQueueRepository
{
    private readonly IDbConnection _connection;

    public DapperEmailQueueRepository(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<int> EnqueueAsync(EmailQueue email, CancellationToken ct = default)
    {
        var emailQueueId = await _connection.QueryFirstOrDefaultAsync<int>(
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
            commandType: CommandType.StoredProcedure,
            commandTimeout: 10);

        return emailQueueId;
    }

    public async Task<IEnumerable<EmailQueue>> GetPendingAsync(int batchSize = 50, int maxAttempts = 3, CancellationToken ct = default)
    {
        var emails = await _connection.QueryAsync<EmailQueue>(
            "Shared.sp_GetPendingEmails",
            new { BatchSize = batchSize, MaxAttempts = maxAttempts },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 10);

        return emails;
    }

    public async Task MarkSentAsync(int emailQueueId, CancellationToken ct = default)
    {
        await _connection.ExecuteAsync(
            "Shared.sp_MarkEmailSent",
            new { EmailQueueId = emailQueueId },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 10);
    }

    public async Task MarkFailedAsync(int emailQueueId, string? failureReason, CancellationToken ct = default)
    {
        await _connection.ExecuteAsync(
            "Shared.sp_MarkEmailFailed",
            new { EmailQueueId = emailQueueId, FailureReason = failureReason },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 10);
    }
}
