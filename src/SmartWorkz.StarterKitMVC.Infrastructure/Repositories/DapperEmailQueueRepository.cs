using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for EmailQueue persistence.
/// Implements IEmailQueueRepository using Shared.sp_* stored procedures.
/// </summary>
public sealed class DapperEmailQueueRepository : IEmailQueueRepository
{
    private readonly string _connectionString;

    public DapperEmailQueueRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    private IDbConnection GetConnection() => new SqlConnection(_connectionString);

    public async Task<int> EnqueueAsync(EmailQueue email, CancellationToken ct = default)
    {
        using var connection = GetConnection();
        var emailQueueId = await connection.QueryFirstOrDefaultAsync<int>(
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
        using var connection = GetConnection();
        var emails = await connection.QueryAsync<EmailQueue>(
            "Shared.sp_GetPendingEmails",
            new { BatchSize = batchSize, MaxAttempts = maxAttempts },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 10);

        return emails;
    }

    public async Task MarkSentAsync(int emailQueueId, CancellationToken ct = default)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Shared.sp_MarkEmailSent",
            new { EmailQueueId = emailQueueId },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 10);
    }

    public async Task MarkFailedAsync(int emailQueueId, string? failureReason, CancellationToken ct = default)
    {
        using var connection = GetConnection();
        await connection.ExecuteAsync(
            "Shared.sp_MarkEmailFailed",
            new { EmailQueueId = emailQueueId, FailureReason = failureReason },
            commandType: CommandType.StoredProcedure,
            commandTimeout: 10);
    }
}
