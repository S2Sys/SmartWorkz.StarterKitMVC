using System.Reflection;

namespace SmartWorkz.Mobile.Data.Database.SQLite;

/// <summary>
/// Manages database migrations with version tracking.
/// Discovers migrations from assembly and executes them in order.
/// </summary>
internal class DbMigrationRunner
{
    private readonly IDatabase _database;
    private const string MigrationHistoryTable = "__MigrationHistory";

    public DbMigrationRunner(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    /// <summary>
    /// Run all pending migrations.
    /// </summary>
    public async Task RunMigrationsAsync(CancellationToken cancellationToken = default)
    {
        if (!_database.IsConnected)
            throw new InvalidOperationException("Database is not connected.");

        // Ensure migration history table exists
        await CreateMigrationHistoryTableAsync(cancellationToken);

        // Get all migration classes from assembly
        var migrations = DiscoverMigrations();

        // Get already applied migration versions
        var appliedVersions = await GetAppliedMigrationsAsync(cancellationToken);

        // Execute pending migrations
        foreach (var migration in migrations.OrderBy(m => m.Version))
        {
            if (!appliedVersions.Contains(migration.Version))
            {
                await ExecuteMigrationAsync(migration, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Get the current schema version.
    /// </summary>
    public async Task<int> GetCurrentVersionAsync(CancellationToken cancellationToken = default)
    {
        if (!_database.IsConnected)
            return 0;

        try
        {
            var result = await _database.QueryFirstOrDefaultAsync<dynamic>(
                $"SELECT MAX(Version) as Version FROM {MigrationHistoryTable}",
                cancellationToken: cancellationToken);

            if (result == null)
                return 0;

            // Handle dynamic object property access
            if (result is IDictionary<string, object?> dict && dict.TryGetValue("Version", out var version))
            {
                return version == null ? 0 : Convert.ToInt32(version);
            }

            // Handle reflection-based property access
            var versionProp = result.GetType().GetProperty("Version");
            if (versionProp?.GetValue(result) is int versionValue)
                return versionValue;

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Create migration history table if it doesn't exist.
    /// </summary>
    private async Task CreateMigrationHistoryTableAsync(CancellationToken cancellationToken)
    {
        const string createTableSql = $@"
CREATE TABLE IF NOT EXISTS {MigrationHistoryTable} (
    Id TEXT PRIMARY KEY,
    Version INTEGER NOT NULL UNIQUE,
    Description TEXT NOT NULL,
    AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP
)";

        await _database.ExecuteAsync(createTableSql, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get list of already applied migration versions.
    /// </summary>
    private async Task<List<int>> GetAppliedMigrationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var results = await _database.QueryAsync<dynamic>(
                $"SELECT Version FROM {MigrationHistoryTable} ORDER BY Version",
                cancellationToken: cancellationToken);

            var versions = new List<int>();
            foreach (var result in results)
            {
                if (result is IDictionary<string, object?> dict && dict.TryGetValue("Version", out var version))
                {
                    if (version != null)
                        versions.Add(Convert.ToInt32(version));
                }
                else
                {
                    var versionProp = result.GetType().GetProperty("Version");
                    if (versionProp?.GetValue(result) is int versionValue)
                        versions.Add(versionValue);
                }
            }

            return versions;
        }
        catch
        {
            return new List<int>();
        }
    }

    /// <summary>
    /// Execute a single migration.
    /// </summary>
    private async Task ExecuteMigrationAsync(DbMigration migration, CancellationToken cancellationToken)
    {
        var connection = _database.CreateConnection();

        try
        {
            // Begin transaction for migration
            var transaction = await _database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Execute migration
                await migration.UpAsync(connection);

                // Record migration in history
                await RecordMigrationAsync(migration, cancellationToken);

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error executing migration {migration.Version}: {migration.Description}", ex);
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    /// <summary>
    /// Record migration in history table.
    /// </summary>
    private async Task RecordMigrationAsync(DbMigration migration, CancellationToken cancellationToken)
    {
        const string insertSql = $@"
INSERT INTO {MigrationHistoryTable} (Id, Version, Description, AppliedAt)
VALUES (@id, @version, @description, @appliedAt)";

        var parameters = new Dictionary<string, object?>
        {
            { "@id", Guid.NewGuid().ToString() },
            { "@version", migration.Version },
            { "@description", migration.Description },
            { "@appliedAt", DateTime.UtcNow }
        };

        await _database.ExecuteAsync(insertSql, parameters, cancellationToken);
    }

    /// <summary>
    /// Discover all migration classes in the assembly.
    /// </summary>
    private static List<DbMigration> DiscoverMigrations()
    {
        var migrations = new List<DbMigration>();

        try
        {
            // Get all types from SmartWorkz.Mobile assembly
            var assembly = typeof(DbMigration).Assembly;
            var migrationTypes = assembly.GetTypes()
                .Where(t => typeof(DbMigration).IsAssignableFrom(t) &&
                            !t.IsAbstract &&
                            t != typeof(DbMigration))
                .OrderBy(t => t.Name);

            foreach (var type in migrationTypes)
            {
                var instance = Activator.CreateInstance(type) as DbMigration;
                if (instance != null)
                {
                    migrations.Add(instance);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Error discovering migrations from assembly.", ex);
        }

        return migrations;
    }
}
