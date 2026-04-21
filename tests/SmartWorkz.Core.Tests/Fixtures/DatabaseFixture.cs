namespace SmartWorkz.Core.Tests.Fixtures;

using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Xunit;

/// <summary>
/// xUnit fixture for integration tests that need a real test database.
/// Creates a fresh test database for each test, drops it after test completes.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly string _connectionString;
    private readonly string _testDatabaseName;
    private SqlConnection? _connection;

    public string ConnectionString => _connectionString;

    public DatabaseFixture()
    {
        _testDatabaseName = $"SmartWorkzTest_{Guid.NewGuid():N}";
        _connectionString = $"Server=(localdb)\\mssqllocaldb;Database={_testDatabaseName};Integrated Security=true;";
    }

    public async Task InitializeAsync()
    {
        // Create test database
        using (var connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Integrated Security=true;"))
        {
            await connection.OpenAsync();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"CREATE DATABASE [{_testDatabaseName}]";
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Create schema
        _connection = new SqlConnection(_connectionString);
        await _connection.OpenAsync();
        await CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        _connection?.Dispose();

        // Drop test database
        using (var connection = new SqlConnection("Server=(localdb)\\mssqllocaldb;Integrated Security=true;"))
        {
            await connection.OpenAsync();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $@"
                    ALTER DATABASE [{_testDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{_testDatabaseName}];
                ";
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    private async Task CreateSchemaAsync()
    {
        const string schema = @"
            -- Domain Events table
            CREATE TABLE [DomainEvents] (
                [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                [AggregateId] NVARCHAR(MAX) NOT NULL,
                [EventType] NVARCHAR(MAX) NOT NULL,
                [Payload] NVARCHAR(MAX) NOT NULL,
                [OccurredAt] DATETIMEOFFSET NOT NULL,
                [CreatedAt] DATETIMEOFFSET DEFAULT GETUTCDATE()
            );

            -- Event Store table
            CREATE TABLE [EventStore] (
                [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                [AggregateId] NVARCHAR(MAX) NOT NULL,
                [AggregateType] NVARCHAR(MAX) NOT NULL,
                [EventType] NVARCHAR(MAX) NOT NULL,
                [EventData] NVARCHAR(MAX) NOT NULL,
                [Version] INT NOT NULL,
                [Timestamp] DATETIMEOFFSET DEFAULT GETUTCDATE()
            );

            -- Event Store Snapshots
            CREATE TABLE [EventStoreSnapshots] (
                [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                [AggregateId] NVARCHAR(MAX) NOT NULL,
                [AggregateType] NVARCHAR(MAX) NOT NULL,
                [SnapshotData] NVARCHAR(MAX) NOT NULL,
                [Version] INT NOT NULL,
                [CreatedAt] DATETIMEOFFSET DEFAULT GETUTCDATE()
            );

            -- Background Jobs (Hangfire uses its own schema, this is for our tracking)
            CREATE TABLE [BackgroundJobs] (
                [Id] NVARCHAR(MAX) PRIMARY KEY,
                [Type] NVARCHAR(MAX) NOT NULL,
                [Status] NVARCHAR(50) NOT NULL,
                [CreatedAt] DATETIMEOFFSET DEFAULT GETUTCDATE()
            );

            -- File Storage metadata
            CREATE TABLE [FileMetadata] (
                [Id] UNIQUEIDENTIFIER PRIMARY KEY,
                [Path] NVARCHAR(MAX) NOT NULL,
                [FileName] NVARCHAR(MAX) NOT NULL,
                [SizeBytes] BIGINT NOT NULL,
                [ContentType] NVARCHAR(MAX),
                [CreatedAt] DATETIMEOFFSET DEFAULT GETUTCDATE(),
                [ModifiedAt] DATETIMEOFFSET DEFAULT GETUTCDATE()
            );

            -- Create indexes
            CREATE INDEX [IX_DomainEvents_AggregateId] ON [DomainEvents] ([AggregateId]);
            CREATE INDEX [IX_EventStore_AggregateId] ON [EventStore] ([AggregateId]);
            CREATE INDEX [IX_EventStore_Version] ON [EventStore] ([Version]);
        ";

        using (var cmd = _connection!.CreateCommand())
        {
            cmd.CommandText = schema;
            await cmd.ExecuteNonQueryAsync();
        }
    }

    /// <summary>Get connection for tests.</summary>
    public IDbConnection GetConnection() => _connection!;

    /// <summary>Execute raw SQL command (for seeding/cleanup).</summary>
    public async Task ExecuteAsync(string sql, object? param = null)
    {
        await _connection!.ExecuteAsync(sql, param);
    }

    /// <summary>Query and return results.</summary>
    public async Task<List<T>> QueryAsync<T>(string sql, object? param = null)
    {
        return (await _connection!.QueryAsync<T>(sql, param)).ToList();
    }

    /// <summary>Clear all test data.</summary>
    public async Task ClearAsync()
    {
        const string cleanup = @"
            DELETE FROM [FileMetadata];
            DELETE FROM [BackgroundJobs];
            DELETE FROM [EventStoreSnapshots];
            DELETE FROM [EventStore];
            DELETE FROM [DomainEvents];
        ";

        using (var cmd = _connection!.CreateCommand())
        {
            cmd.CommandText = cleanup;
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
