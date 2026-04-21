namespace SmartWorkz.Core.Tests.Fixtures;

using SmartWorkz.Core.Tests.Helpers;
using Xunit;

/// <summary>
/// Unit tests for TestDataBuilder and AssertionHelpers that don't require a database.
/// These tests verify that the builders and assertion helpers work correctly.
/// </summary>
public class TestDataBuilderUnitTests
{
    [Fact]
    public void AssertionHelpers_AssertEventProperties_WithValidEvent_Succeeds()
    {
        // Arrange
        dynamic evt = new
        {
            EventType = "UserCreated",
            AggregateId = "user-123",
            OccurredAt = DateTimeOffset.UtcNow
        };

        // Act & Assert - Should not throw
        AssertionHelpers.AssertEventProperties(evt, "UserCreated", "user-123");
    }

    [Fact]
    public void AssertionHelpers_AssertEventsEqual_WithIdenticalEvents_Succeeds()
    {
        // Arrange
        dynamic expected = new
        {
            EventType = "OrderPlaced",
            AggregateId = "order-456",
            Payload = @"{ ""Amount"": 100 }"
        };

        dynamic actual = new
        {
            EventType = "OrderPlaced",
            AggregateId = "order-456",
            Payload = @"{ ""Amount"": 100 }"
        };

        // Act & Assert - Should not throw
        AssertionHelpers.AssertEventsEqual(expected, actual);
    }

    [Fact]
    public void AssertionHelpers_AssertEventPayloadContains_WithValidPayload_Succeeds()
    {
        // Arrange
        dynamic evt = new
        {
            Payload = @"{ ""Name"": ""John Doe"", ""Email"": ""john@example.com"" }"
        };

        // Act & Assert - Should not throw
        AssertionHelpers.AssertEventPayloadContains(evt, "Name", "John Doe");
    }

    [Fact]
    public void AssertionHelpers_AssertEventSequence_WithValidSequence_Succeeds()
    {
        // Arrange
        dynamic event1 = new { EventType = "OrderPlaced" };
        dynamic event2 = new { EventType = "PaymentProcessed" };
        dynamic event3 = new { EventType = "OrderShipped" };

        var events = new[] { event1, event2, event3 };

        // Act & Assert - Should not throw
        AssertionHelpers.AssertEventSequence(events, "OrderPlaced", "PaymentProcessed", "OrderShipped");
    }

    [Fact]
    public void AssertionHelpers_AssertCommandResult_WithSuccessTrue_Succeeds()
    {
        // Arrange
        dynamic result = new { IsSuccess = true };

        // Act & Assert - Should not throw
        AssertionHelpers.AssertCommandResult(result, shouldSucceed: true);
    }

    [Fact]
    public void AssertionHelpers_AssertQueryResult_WithValidResult_Succeeds()
    {
        // Arrange
        var result = new { Id = 1, Name = "Test" };

        // Act & Assert - Should not throw
        AssertionHelpers.AssertQueryResult(result, r => r.Id == 1);
    }

    [Fact]
    public void AssertionHelpers_AssertFileMetadata_WithValidMetadata_Succeeds()
    {
        // Arrange
        dynamic metadata = new
        {
            FileName = "document.pdf",
            ContentType = "application/pdf",
            SizeBytes = 5120L
        };

        // Act & Assert - Should not throw
        AssertionHelpers.AssertFileMetadata(
            metadata,
            expectedFileName: "document.pdf",
            expectedContentType: "application/pdf",
            minSizeBytes: 4000,
            maxSizeBytes: 6000
        );
    }

    [Fact]
    public void AssertionHelpers_AssertEventStoreCount_WithMatchingCount_Succeeds()
    {
        // Act & Assert - Should not throw
        AssertionHelpers.AssertEventStoreCount(5, 5);
    }

    [Fact]
    public void AssertionHelpers_AssertSagaCompleted_WithCompletedSaga_Succeeds()
    {
        // Arrange
        dynamic sagaResult = new
        {
            AggregateId = "saga-001",
            Status = "Completed"
        };

        // Act & Assert - Should not throw
        AssertionHelpers.AssertSagaCompleted(sagaResult, "saga-001");
    }

    [Fact]
    public void AssertionHelpers_AssertSagaCompensated_WithCompensatedSaga_Succeeds()
    {
        // Arrange
        dynamic sagaResult = new
        {
            Status = "Compensated"
        };

        // Act & Assert - Should not throw
        AssertionHelpers.AssertSagaCompensated(sagaResult);
    }
}
