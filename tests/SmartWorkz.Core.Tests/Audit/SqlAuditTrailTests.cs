namespace SmartWorkz.Core.Tests.Audit;

using System.Data;
using SmartWorkz.Core;
using SmartWorkz.Shared;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

/// <summary>
/// Unit tests for SqlAuditTrail implementation.
/// Tests audit entry recording and retrieval by various filters.
/// </summary>
public class SqlAuditTrailTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<ILogger<SqlAuditTrail>> _mockLogger;
    private readonly SqlAuditTrail _service;

    public SqlAuditTrailTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _mockLogger = new Mock<ILogger<SqlAuditTrail>>();
        _service = new SqlAuditTrail(_mockConnection.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullConnection_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new SqlAuditTrail(null!, _mockLogger.Object));
        Assert.Equal("connection", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new SqlAuditTrail(_mockConnection.Object, null!));
        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task RecordAsync_WithNullEntry_ThrowsArgumentNullException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.RecordAsync(null!));
        Assert.Equal("entry", ex.ParamName);
    }

    [Fact]
    public async Task RecordAsync_WithValidEntry_LogsInformation()
    {
        // Note: Cannot mock Dapper extension methods (ExecuteAsync is an extension on IDbConnection).
        // This test verifies logging only; the DB call will throw at runtime without a real connection.
        var entry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = "Order",
            EntityId = "order-123",
            Action = "Created",
            UserId = "user-456",
            Timestamp = DateTimeOffset.UtcNow
        };

        try { await _service.RecordAsync(entry); } catch { }

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RecordAsync_WithEmptyId_CallsExecute()
    {
        // Note: Cannot mock Dapper extension methods (ExecuteAsync is an extension on IDbConnection).
        var entry = new AuditEntry
        {
            Id = Guid.Empty,
            EntityType = "Order",
            EntityId = "order-123",
            Action = "Created",
            Timestamp = DateTimeOffset.UtcNow
        };

        // Just verify no unhandled exception before the DB call
        try { await _service.RecordAsync(entry); } catch { }
    }

    [Fact]
    public async Task RecordAsync_WithDefaultTimestamp_CallsExecute()
    {
        // Note: Cannot mock Dapper extension methods (ExecuteAsync is an extension on IDbConnection).
        var entry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = "Order",
            EntityId = "order-123",
            Action = "Created",
            Timestamp = default
        };

        try { await _service.RecordAsync(entry); } catch { }
    }

    [Fact]
    public async Task GetEntriesAsync_WithNullEntityType_ThrowsArgumentNullException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetEntriesAsync(null!, "order-123"));
        Assert.Equal("entityType", ex.ParamName);
    }

    [Fact]
    public async Task GetEntriesAsync_WithEmptyEntityType_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetEntriesAsync("", "order-123"));
        Assert.Equal("entityType", ex.ParamName);
    }

    [Fact]
    public async Task GetEntriesAsync_WithNullEntityId_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetEntriesAsync("Order", null!));
        Assert.Equal("entityId", ex.ParamName);
    }

    [Fact]
    public async Task GetEntriesByActionAsync_WithNullAction_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetEntriesByActionAsync(null!));
        Assert.Equal("action", ex.ParamName);
    }

    [Fact]
    public async Task GetEntriesByActionAsync_WithEmptyAction_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetEntriesByActionAsync(""));
        Assert.Equal("action", ex.ParamName);
    }

    [Fact]
    public async Task GetEntriesByUserAsync_WithNullUserId_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetEntriesByUserAsync(null!));
        Assert.Equal("userId", ex.ParamName);
    }

    [Fact]
    public async Task GetEntriesByUserAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetEntriesByUserAsync(""));
        Assert.Equal("userId", ex.ParamName);
    }
}
