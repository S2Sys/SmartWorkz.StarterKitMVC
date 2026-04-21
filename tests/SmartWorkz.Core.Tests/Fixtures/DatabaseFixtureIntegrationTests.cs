namespace SmartWorkz.Core.Tests.Fixtures;

using SmartWorkz.Core.Tests.Helpers;
using System.Data;
using Xunit;

public class DatabaseFixtureIntegrationTests : IAsyncLifetime
{
    private DatabaseFixture? _fixture;

    public async Task InitializeAsync()
    {
        _fixture = new DatabaseFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        if (_fixture != null)
            await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task DatabaseFixture_CreatesSchema_Successfully()
    {
        // Act
        var connection = _fixture!.GetConnection();

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public async Task TestDataBuilder_SavesDomainEvent_Successfully()
    {
        // Arrange
        var builder = new TestDataBuilder(_fixture!);

        // Act
        var eventId = await builder.SaveDomainEventAsync(
            aggregateId: "test-aggregate-123",
            eventType: "UserCreated",
            payload: @"{ ""Name"": ""Test User"" }"
        );

        // Assert
        Assert.NotEqual(Guid.Empty, eventId);

        var events = await _fixture!.QueryAsync<dynamic>("SELECT * FROM [DomainEvents] WHERE Id = @Id", new { Id = eventId });
        Assert.Single(events);
        Assert.Equal("UserCreated", events[0].EventType);
    }

    [Fact]
    public async Task TestDataBuilder_BuildsEventWithFluentApi_Successfully()
    {
        // Arrange
        var builder = new TestDataBuilder(_fixture!);

        // Act
        var eventId = await builder
            .BuildDomainEvent()
            .WithAggregateId("order-456")
            .WithEventType("OrderPlaced")
            .SaveAsync();

        // Assert
        var events = await _fixture!.QueryAsync<dynamic>("SELECT * FROM [DomainEvents] WHERE Id = @Id", new { Id = eventId });
        Assert.Single(events);
        AssertionHelpers.AssertEventProperties(events[0], "OrderPlaced", "order-456");
    }

    [Fact]
    public async Task AssertionHelpers_ValidateEventProperties_Successfully()
    {
        // Arrange
        var builder = new TestDataBuilder(_fixture!);
        var eventId = await builder.SaveDomainEventAsync(
            aggregateId: "payment-789",
            eventType: "PaymentProcessed"
        );

        var events = await _fixture!.QueryAsync<dynamic>("SELECT * FROM [DomainEvents] WHERE Id = @Id", new { Id = eventId });

        // Act & Assert
        AssertionHelpers.AssertEventProperties(events[0], "PaymentProcessed", "payment-789");
        AssertionHelpers.AssertEventSequence(events, "PaymentProcessed");
    }

    [Fact]
    public async Task DatabaseFixture_ClearsData_Successfully()
    {
        // Arrange
        var builder = new TestDataBuilder(_fixture!);
        await builder.SaveDomainEventAsync();
        await builder.SaveDomainEventAsync();

        var countBefore = await _fixture!.QueryAsync<dynamic>("SELECT COUNT(*) as Count FROM [DomainEvents]");
        Assert.Equal(2, (int)countBefore[0].Count);

        // Act
        await _fixture.ClearAsync();

        // Assert
        var countAfter = await _fixture!.QueryAsync<dynamic>("SELECT COUNT(*) as Count FROM [DomainEvents]");
        Assert.Equal(0, (int)countAfter[0].Count);
    }

    [Fact]
    public async Task TestDataBuilder_SavesFileMetadata_Successfully()
    {
        // Arrange
        var builder = new TestDataBuilder(_fixture!);

        // Act
        var fileId = await builder
            .BuildFileMetadata()
            .WithFileName("test-document.pdf")
            .WithContentType("application/pdf")
            .WithSizeBytes(5120)
            .SaveAsync();

        // Assert
        Assert.NotEqual(Guid.Empty, fileId);
        var files = await _fixture!.QueryAsync<dynamic>("SELECT * FROM [FileMetadata] WHERE Id = @Id", new { Id = fileId });
        Assert.Single(files);
        AssertionHelpers.AssertFileMetadata(files[0], "test-document.pdf", "application/pdf", 5000, 6000);
    }

    [Fact]
    public async Task TestDataBuilder_SavesEventStoreRecord_Successfully()
    {
        // Arrange
        var builder = new TestDataBuilder(_fixture!);

        // Act
        var recordId = await builder
            .BuildEventStoreRecord()
            .WithAggregateId("invoice-001")
            .WithAggregateType("Invoice")
            .WithEventType("InvoiceCreated")
            .WithVersion(1)
            .SaveAsync();

        // Assert
        Assert.NotEqual(Guid.Empty, recordId);
        var records = await _fixture!.QueryAsync<dynamic>("SELECT * FROM [EventStore] WHERE Id = @Id", new { Id = recordId });
        Assert.Single(records);
        Assert.Equal("InvoiceCreated", records[0].EventType);
        Assert.Equal("Invoice", records[0].AggregateType);
    }

    [Fact]
    public async Task TestDataBuilder_SavesBackgroundJob_Successfully()
    {
        // Arrange
        var builder = new TestDataBuilder(_fixture!);

        // Act
        var jobId = await builder
            .BuildBackgroundJob()
            .WithType("SendEmailJob")
            .WithStatus("Processing")
            .SaveAsync();

        // Assert
        Assert.NotEmpty(jobId);
        var jobs = await _fixture!.QueryAsync<dynamic>("SELECT * FROM [BackgroundJobs] WHERE Id = @Id", new { Id = jobId });
        Assert.Single(jobs);
        Assert.Equal("SendEmailJob", jobs[0].Type);
        Assert.Equal("Processing", jobs[0].Status);
    }

    [Fact]
    public async Task AssertionHelpers_ValidateFileMetadata_Successfully()
    {
        // Arrange
        var builder = new TestDataBuilder(_fixture!);
        var fileId = await builder.SaveFileMetadataAsync(
            fileName: "image.png",
            contentType: "image/png",
            sizeBytes: 2048
        );

        var files = await _fixture!.QueryAsync<dynamic>("SELECT * FROM [FileMetadata] WHERE Id = @Id", new { Id = fileId });

        // Act & Assert
        AssertionHelpers.AssertFileMetadata(files[0], "image.png", "image/png", 2000, 3000);
    }
}
