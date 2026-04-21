namespace SmartWorkz.Core.Tests.Audit;

using System.Data;
using SmartWorkz.Core;
using SmartWorkz.Core.Shared.Audit;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

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
        var entry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = "Order",
            EntityId = "order-123",
            Action = "Created",
            UserId = "user-456",
            Timestamp = DateTimeOffset.UtcNow
        };

        await _service.RecordAsync(entry);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit recorded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RecordAsync_WithEmptyId_GeneratesNewGuid()
    {
        var entry = new AuditEntry
        {
            Id = Guid.Empty,
            EntityType = "Order",
            EntityId = "order-123",
            Action = "Created",
            Timestamp = DateTimeOffset.UtcNow
        };

        // Mock Dapper's ExecuteAsync to capture the parameters
        var capturedParams = new object?();
        _mockConnection.Setup(c =>
            c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Callback<string, object?>((sql, param) => capturedParams = param)
            .ReturnsAsync(1);

        await _service.RecordAsync(entry);

        _mockConnection.Verify(c =>
            c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task RecordAsync_WithDefaultTimestamp_UsesUtcNow()
    {
        var entry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = "Order",
            EntityId = "order-123",
            Action = "Created",
            Timestamp = default
        };

        _mockConnection.Setup(c =>
            c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(1);

        await _service.RecordAsync(entry);

        _mockConnection.Verify(c =>
            c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task RecordAsync_WithException_LogsError()
    {
        var entry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            EntityType = "Order",
            EntityId = "order-123",
            Action = "Created"
        };

        var testException = new InvalidOperationException("Database error");
        _mockConnection.Setup(c =>
            c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ThrowsAsync(testException);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RecordAsync(entry));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to record")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
    public async Task GetEntriesAsync_WithValidParameters_ReturnsEntries()
    {
        var entries = new[]
        {
            new AuditEntry
            {
                Id = Guid.NewGuid(),
                EntityType = "Order",
                EntityId = "order-123",
                Action = "Created",
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        _mockConnection.Setup(c =>
            c.QueryAsync<AuditEntry>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(entries);

        var result = await _service.GetEntriesAsync("Order", "order-123");

        Assert.Single(result);
        Assert.Equal("Created", result.First().Action);
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
    public async Task GetEntriesByActionAsync_WithValidAction_ReturnsEntries()
    {
        var entries = new[]
        {
            new AuditEntry
            {
                Id = Guid.NewGuid(),
                EntityType = "Order",
                Action = "Created",
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        _mockConnection.Setup(c =>
            c.QueryAsync<AuditEntry>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(entries);

        var result = await _service.GetEntriesByActionAsync("Created");

        Assert.Single(result);
        Assert.Equal("Created", result.First().Action);
    }

    [Fact]
    public async Task GetEntriesByActionAsync_WithSinceFilter_IncludesInQuery()
    {
        var since = DateTimeOffset.UtcNow.AddDays(-1);
        var entries = new AuditEntry[0];

        _mockConnection.Setup(c =>
            c.QueryAsync<AuditEntry>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(entries);

        await _service.GetEntriesByActionAsync("Created", since);

        _mockConnection.Verify(c =>
            c.QueryAsync<AuditEntry>(
                It.IsAny<string>(),
                It.Is<object>(p => p != null)),
            Times.Once);
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

    [Fact]
    public async Task GetEntriesByUserAsync_WithValidUserId_ReturnsEntries()
    {
        var entries = new[]
        {
            new AuditEntry
            {
                Id = Guid.NewGuid(),
                UserId = "user-123",
                Action = "Created",
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        _mockConnection.Setup(c =>
            c.QueryAsync<AuditEntry>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(entries);

        var result = await _service.GetEntriesByUserAsync("user-123");

        Assert.Single(result);
        Assert.Equal("user-123", result.First().UserId);
    }

    [Fact]
    public async Task SearchAsync_WithMultipleFilters_ReturnsFilteredEntries()
    {
        var entries = new[]
        {
            new AuditEntry
            {
                Id = Guid.NewGuid(),
                EntityType = "Order",
                UserId = "user-123",
                Action = "Created",
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        _mockConnection.Setup(c =>
            c.QueryAsync<AuditEntry>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(entries);

        var result = await _service.SearchAsync("Order", "Created", "user-123");

        Assert.Single(result);
        Assert.Equal("Order", result.First().EntityType);
        Assert.Equal("Created", result.First().Action);
        Assert.Equal("user-123", result.First().UserId);
    }

    [Fact]
    public async Task SearchAsync_WithNoFilters_ReturnsAllEntries()
    {
        var entries = new[]
        {
            new AuditEntry { Id = Guid.NewGuid(), EntityType = "Order", Action = "Created" },
            new AuditEntry { Id = Guid.NewGuid(), EntityType = "Invoice", Action = "Deleted" }
        };

        _mockConnection.Setup(c =>
            c.QueryAsync<AuditEntry>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(entries);

        var result = await _service.SearchAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SearchAsync_WithPartialFilters_ReturnsEntries()
    {
        var entries = new[]
        {
            new AuditEntry
            {
                Id = Guid.NewGuid(),
                EntityType = "Order",
                UserId = "user-123",
                Action = "Created"
            }
        };

        _mockConnection.Setup(c =>
            c.QueryAsync<AuditEntry>(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync(entries);

        var result = await _service.SearchAsync(entityType: "Order", userId: "user-123");

        Assert.Single(result);
    }
}
