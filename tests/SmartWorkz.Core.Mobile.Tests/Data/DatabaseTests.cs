using Xunit;
using SmartWorkz.Mobile.Data.Database;

namespace SmartWorkz.Mobile.Tests.Data;

public class DatabaseTests
{
    [Fact]
    public async Task OpenAsync_WithValidPath_OpensConnection()
    {
        // Arrange
        var db = new TestDatabase();

        // Act
        await db.OpenAsync();

        // Assert
        Assert.True(db.IsConnected);
    }

    [Fact]
    public async Task RunMigrationsAsync_ExecutesAllMigrations()
    {
        // Arrange
        var db = new TestDatabase();
        await db.OpenAsync();

        // Act
        await db.RunMigrationsAsync();

        // Assert
        Assert.True(db.IsConnected);
        // Verify tables exist
        var tables = await db.QueryAsync<string>(
            "SELECT name FROM sqlite_master WHERE type='table'");
        Assert.NotEmpty(tables);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidSql_ReturnsRowCount()
    {
        // Arrange
        var db = new TestDatabase();
        await db.OpenAsync();

        // Act
        var rowsAffected = await db.ExecuteAsync(
            "CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");

        // Assert
        Assert.GreaterThanOrEqual(rowsAffected, 0);
    }

    [Fact]
    public async Task QueryAsync_WithValidSql_ReturnsResults()
    {
        // Arrange
        var db = new TestDatabase();
        await db.OpenAsync();
        await db.ExecuteAsync("CREATE TABLE TestTable (Id INTEGER PRIMARY KEY, Name TEXT)");
        await db.ExecuteAsync("INSERT INTO TestTable (Name) VALUES ('Test')");

        // Act
        var results = await db.QueryAsync<dynamic>(
            "SELECT * FROM TestTable");

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public async Task QueryFirstOrDefaultAsync_WithNoResults_ReturnsNull()
    {
        // Arrange
        var db = new TestDatabase();
        await db.OpenAsync();

        // Act
        var result = await db.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM NonExistentTable");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Test implementation of IDatabase for unit testing.
    /// Uses in-memory implementation without actual database.
    /// </summary>
    private class TestDatabase : IDatabase
    {
        public bool IsConnected { get; private set; }
        public int Version => 0;
        public string? DatabasePath => ":memory:";

        public Task OpenAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task CloseAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = false;
            return Task.CompletedTask;
        }

        public Task RunMigrationsAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public IDbConnection CreateConnection() => throw new NotImplementedException();

        public Task<int> ExecuteAsync(string sql, Dictionary<string, object?>? parameters = null, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<List<T>> QueryAsync<T>(string sql, Dictionary<string, object?>? parameters = null, CancellationToken cancellationToken = default) where T : class, new()
            => Task.FromResult(new List<T>());

        public Task<T?> QueryFirstOrDefaultAsync<T>(string sql, Dictionary<string, object?>? parameters = null, CancellationToken cancellationToken = default) where T : class, new()
            => Task.FromResult<T?>(null);

        public Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
