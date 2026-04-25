using Xunit;
using SmartWorkz.Mobile.Data.Database;
using SmartWorkz.Mobile.Data.Database.SQLite;
using SmartWorkz.Mobile.Data.Database.Migrations;
using System.Reflection;

namespace SmartWorkz.Mobile.Tests.Data;

/// <summary>
/// Comprehensive test suite for SQLite database implementation.
/// Tests connection lifecycle, transactions, migrations, and query execution.
/// </summary>
public class SqliteDatabaseTests : IAsyncLifetime
{
    private string _testDatabasePath = null!;
    private IDatabase _database = null!;

    public async Task InitializeAsync()
    {
        // Create a temporary test database file
        _testDatabasePath = Path.Combine(
            Path.GetTempPath(),
            $"test_{Guid.NewGuid():N}.db");

        _database = new SqliteDatabase(_testDatabasePath);
    }

    public async Task DisposeAsync()
    {
        // Clean up test database
        if (_database is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }

        if (File.Exists(_testDatabasePath))
        {
            try
            {
                File.Delete(_testDatabasePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task OpenAsync_WithValidPath_OpensConnection()
    {
        // Act
        await _database.OpenAsync();

        // Assert
        Assert.True(_database.IsConnected);
    }

    [Fact]
    public async Task OpenAsync_WithValidPath_SetsCorrectDatabasePath()
    {
        // Act
        await _database.OpenAsync();

        // Assert
        Assert.Equal(_testDatabasePath, _database.DatabasePath);
    }

    [Fact]
    public async Task CloseAsync_AfterOpen_ClosesConnection()
    {
        // Arrange
        await _database.OpenAsync();
        Assert.True(_database.IsConnected);

        // Act
        await _database.CloseAsync();

        // Assert
        Assert.False(_database.IsConnected);
    }

    [Fact]
    public async Task ExecuteAsync_WithCreateTableSql_ReturnsZeroOrGreater()
    {
        // Arrange
        await _database.OpenAsync();

        // Act
        var rowsAffected = await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");

        // Assert
        Assert.GreaterThanOrEqual(rowsAffected, 0);
    }

    [Fact]
    public async Task ExecuteAsync_WithInsertSql_InsertsData()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");

        // Act
        var rowsAffected = await _database.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES ('Test')");

        // Assert
        Assert.GreaterThan(rowsAffected, 0);
    }

    [Fact]
    public async Task ExecuteAsync_WithParameters_ExecutesCorrectly()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");

        // Act
        var parameters = new Dictionary<string, object?> { { "@name", "TestValue" } };
        var rowsAffected = await _database.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES (@name)", parameters);

        // Assert
        Assert.GreaterThan(rowsAffected, 0);
    }

    [Fact]
    public async Task QueryAsync_WithValidSql_ReturnsResults()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");
        await _database.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES ('Test1')");
        await _database.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES ('Test2')");

        // Act
        var results = await _database.QueryAsync<dynamic>(
            "SELECT * FROM TestTable ORDER BY Id");

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task QueryAsync_WithEmptyTable_ReturnsEmptyList()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");

        // Act
        var results = await _database.QueryAsync<dynamic>(
            "SELECT * FROM TestTable");

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task QueryFirstOrDefaultAsync_WithResults_ReturnsFirstRow()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");
        await _database.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES ('Test1')");
        await _database.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES ('Test2')");

        // Act
        var result = await _database.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM TestTable ORDER BY Id");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task QueryFirstOrDefaultAsync_WithNoResults_ReturnsNull()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");

        // Act
        var result = await _database.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM TestTable");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateConnection_ReturnsValidConnection()
    {
        // Arrange
        await _database.OpenAsync();

        // Act
        var connection = _database.CreateConnection();

        // Assert
        Assert.NotNull(connection);
        Assert.IsAssignableFrom<IDbConnection>(connection);
    }

    [Fact]
    public async Task BeginTransactionAsync_ReturnsValidTransaction()
    {
        // Arrange
        await _database.OpenAsync();

        // Act
        var transaction = await _database.BeginTransactionAsync();

        // Assert
        Assert.NotNull(transaction);
        Assert.IsAssignableFrom<IDbTransaction>(transaction);
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task RunMigrationsAsync_CreatesTablesFromMigrations()
    {
        // Arrange
        await _database.OpenAsync();

        // Act
        await _database.RunMigrationsAsync();

        // Assert
        var tables = await _database.QueryAsync<dynamic>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'");
        Assert.NotEmpty(tables);
    }

    [Fact]
    public async Task RunMigrationsAsync_CreatesMigrationHistoryTable()
    {
        // Arrange
        await _database.OpenAsync();

        // Act
        await _database.RunMigrationsAsync();

        // Assert
        var migrationHistoryExists = await _database.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name='__MigrationHistory'");
        Assert.NotNull(migrationHistoryExists);
    }

    [Fact]
    public async Task RunMigrationsAsync_TracksAppliedMigrations()
    {
        // Arrange
        await _database.OpenAsync();

        // Act
        await _database.RunMigrationsAsync();

        // Assert
        var migrationCount = await _database.QueryAsync<dynamic>(
            "SELECT * FROM __MigrationHistory");
        Assert.NotEmpty(migrationCount);
    }

    [Fact]
    public async Task RunMigrationsAsync_DoesNotReapplyMigrations()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.RunMigrationsAsync();

        var initialMigrationCount = (await _database.QueryAsync<dynamic>(
            "SELECT * FROM __MigrationHistory")).Count;

        // Act - Run migrations again
        await _database.RunMigrationsAsync();

        // Assert
        var finalMigrationCount = (await _database.QueryAsync<dynamic>(
            "SELECT * FROM __MigrationHistory")).Count;
        Assert.Equal(initialMigrationCount, finalMigrationCount);
    }

    [Fact]
    public async Task Version_AfterMigrations_MatchesCurrentSchemaVersion()
    {
        // Arrange
        await _database.OpenAsync();

        // Act
        await _database.RunMigrationsAsync();
        var version = _database.Version;

        // Assert
        Assert.GreaterThan(version, 0);
    }

    [Fact]
    public async Task Transaction_Commit_PersistsData()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");

        var transaction = await _database.BeginTransactionAsync();

        // Act
        var conn = transaction.Connection;
        await conn.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES ('TransactionTest')");
        await transaction.CommitAsync();

        // Assert
        var result = await _database.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM TestTable WHERE Name = 'TransactionTest'");
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Transaction_Rollback_DoesNotPersistData()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");

        var transaction = await _database.BeginTransactionAsync();

        // Act
        var conn = transaction.Connection;
        await conn.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES ('RollbackTest')");
        await transaction.RollbackAsync();

        // Assert
        var result = await _database.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM TestTable WHERE Name = 'RollbackTest'");
        Assert.Null(result);
    }

    [Fact]
    public async Task SqliteConnection_ExecuteAsync_ExecutesStatement()
    {
        // Arrange
        await _database.OpenAsync();
        var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");

        // Act
        var rowsAffected = await connection.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES ('Test')");

        // Assert
        Assert.GreaterThan(rowsAffected, 0);
    }

    [Fact]
    public async Task SqliteConnection_QueryAsync_ReturnsResults()
    {
        // Arrange
        await _database.OpenAsync();
        var connection = _database.CreateConnection();
        await connection.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");
        await connection.ExecuteAsync(
            "INSERT INTO TestTable (Name) VALUES ('Test')");

        // Act
        var results = await connection.QueryAsync<dynamic>(
            "SELECT * FROM TestTable");

        // Assert
        Assert.NotEmpty(results);
    }

    [Fact]
    public async Task SqliteConnection_DisposeAsync_ReleasesResources()
    {
        // Arrange
        await _database.OpenAsync();
        var connection = _database.CreateConnection();

        // Act & Assert - Should not throw
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        await _database.OpenAsync();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _database.ExecuteAsync(
                "SELECT 1",
                cancellationToken: cts.Token));
    }

    [Fact]
    public async Task QueryAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        await _database.OpenAsync();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _database.QueryAsync<dynamic>(
                "SELECT 1",
                cancellationToken: cts.Token));
    }

    [Fact]
    public async Task Database_ConcurrentOperations_ExecutesSuccessfully()
    {
        // Arrange
        await _database.OpenAsync();
        await _database.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Value INTEGER)");

        // Act - Run concurrent inserts
        var tasks = Enumerable.Range(0, 10)
            .Select(i => _database.ExecuteAsync(
                "INSERT INTO TestTable (Value) VALUES (@value)",
                new Dictionary<string, object?> { { "@value", i } }))
            .ToList();

        await Task.WhenAll(tasks);

        // Assert
        var results = await _database.QueryAsync<dynamic>(
            "SELECT * FROM TestTable");
        Assert.Equal(10, results.Count);
    }

    [Fact]
    public async Task MigrationRunner_GetsMigrationsFromAssembly()
    {
        // Arrange
        await _database.OpenAsync();

        // Act
        await _database.RunMigrationsAsync();

        // Assert - Verify specific migrations exist
        var allMigrations = await _database.QueryAsync<dynamic>(
            "SELECT Version FROM __MigrationHistory ORDER BY Version");

        Assert.NotEmpty(allMigrations);
        // Should have at least the two migrations we know about
        Assert.True(allMigrations.Count >= 2);
    }

    [Fact]
    public async Task DatabasePath_Property_ReturnsCorrectPath()
    {
        // Arrange & Act
        await _database.OpenAsync();

        // Assert
        Assert.Equal(_testDatabasePath, _database.DatabasePath);
    }

    [Fact]
    public async Task IsConnected_Property_ReflectsState()
    {
        // Act & Assert
        Assert.False(_database.IsConnected);

        await _database.OpenAsync();
        Assert.True(_database.IsConnected);

        await _database.CloseAsync();
        Assert.False(_database.IsConnected);
    }
}
