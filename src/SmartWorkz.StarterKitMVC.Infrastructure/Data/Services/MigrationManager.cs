using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Infrastructure.Data;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Data.Services;

public interface IMigrationManager
{
    Task MigrateAsync();
    Task RollbackAsync(string migrationName);
    Task GetPendingMigrationsAsync();
}

public class MigrationManager : IMigrationManager
{
    private readonly AuthDbContext _authDb;
    private readonly MasterDbContext _masterDb;
    private readonly SharedDbContext _sharedDb;
    private readonly TransactionDbContext _transactionDb;
    private readonly ReportDbContext _reportDb;
    private readonly ILogger<MigrationManager> _logger;

    public MigrationManager(
        AuthDbContext authDb,
        MasterDbContext masterDb,
        SharedDbContext sharedDb,
        TransactionDbContext transactionDb,
        ReportDbContext reportDb,
        ILogger<MigrationManager> logger)
    {
        _authDb = authDb;
        _masterDb = masterDb;
        _sharedDb = sharedDb;
        _transactionDb = transactionDb;
        _reportDb = reportDb;
        _logger = logger;
    }

    public async Task MigrateAsync()
    {
        try
        {
            _logger.LogInformation("Starting database migrations...");

            await _authDb.Database.MigrateAsync();
            _logger.LogInformation("✓ AuthDbContext migrated");

            await _masterDb.Database.MigrateAsync();
            _logger.LogInformation("✓ MasterDbContext migrated");

            await _sharedDb.Database.MigrateAsync();
            _logger.LogInformation("✓ SharedDbContext migrated");

            await _transactionDb.Database.MigrateAsync();
            _logger.LogInformation("✓ TransactionDbContext migrated");

            await _reportDb.Database.MigrateAsync();
            _logger.LogInformation("✓ ReportDbContext migrated");

            _logger.LogInformation("Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database migration failed");
            throw;
        }
    }

    public async Task RollbackAsync(string migrationName)
    {
        try
        {
            _logger.LogWarning($"Rolling back to migration: {migrationName}");

            // Roll back each context using ExecuteSqlInterpolatedAsync for safe parameterization
            await _authDb.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM [dbo].[__EFMigrationsHistory] WHERE MigrationId > {migrationName}");
            _logger.LogInformation("✓ AuthDbContext rolled back");

            await _masterDb.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM [dbo].[__EFMigrationsHistory] WHERE MigrationId > {migrationName}");
            _logger.LogInformation("✓ MasterDbContext rolled back");

            await _sharedDb.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM [dbo].[__EFMigrationsHistory] WHERE MigrationId > {migrationName}");
            _logger.LogInformation("✓ SharedDbContext rolled back");

            await _transactionDb.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM [dbo].[__EFMigrationsHistory] WHERE MigrationId > {migrationName}");
            _logger.LogInformation("✓ TransactionDbContext rolled back");

            await _reportDb.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM [dbo].[__EFMigrationsHistory] WHERE MigrationId > {migrationName}");
            _logger.LogInformation("✓ ReportDbContext rolled back");

            _logger.LogInformation("Rollback completed for all contexts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rollback failed");
            throw;
        }
    }

    public async Task GetPendingMigrationsAsync()
    {
        try
        {
            var authPending = await _authDb.Database.GetPendingMigrationsAsync();
            var masterPending = await _masterDb.Database.GetPendingMigrationsAsync();
            var sharedPending = await _sharedDb.Database.GetPendingMigrationsAsync();
            var transactionPending = await _transactionDb.Database.GetPendingMigrationsAsync();
            var reportPending = await _reportDb.Database.GetPendingMigrationsAsync();

            _logger.LogInformation($"Auth pending: {authPending.Count()}");
            _logger.LogInformation($"Master pending: {masterPending.Count()}");
            _logger.LogInformation($"Shared pending: {sharedPending.Count()}");
            _logger.LogInformation($"Transaction pending: {transactionPending.Count()}");
            _logger.LogInformation($"Report pending: {reportPending.Count()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pending migrations");
            throw;
        }
    }
}
