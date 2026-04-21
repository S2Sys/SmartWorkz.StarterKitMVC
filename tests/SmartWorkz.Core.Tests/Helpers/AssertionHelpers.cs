namespace SmartWorkz.Core.Tests.Helpers;

using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using Xunit;

/// <summary>
/// Domain-specific assertion helpers for testing SmartWorkz components.
/// Provides fluent assertions for events, CQRS results, and domain logic.
/// </summary>
public static class AssertionHelpers
{
    /// <summary>Assert event has expected properties.</summary>
    public static void AssertEventProperties(
        dynamic evt,
        string? expectedEventType = null,
        string? expectedAggregateId = null,
        DateTimeOffset? expectedOccurredAtApprox = null)
    {
        if (expectedEventType != null)
            Assert.Equal(expectedEventType, evt.EventType);

        if (expectedAggregateId != null)
            Assert.Equal(expectedAggregateId, evt.AggregateId);

        if (expectedOccurredAtApprox.HasValue)
        {
            var difference = Math.Abs((evt.OccurredAt - expectedOccurredAtApprox.Value).TotalSeconds);
            Assert.True(difference < 1, "Event OccurredAt differs by more than 1 second");
        }
    }

    /// <summary>Assert two events are equal (by value).</summary>
    public static void AssertEventsEqual(dynamic expected, dynamic actual)
    {
        Assert.Equal(expected.EventType, actual.EventType);
        Assert.Equal(expected.AggregateId, actual.AggregateId);
        Assert.Equal(expected.Payload, actual.Payload);
    }

    /// <summary>Assert event payload contains expected JSON properties.</summary>
    public static void AssertEventPayloadContains(dynamic evt, string expectedKey, string expectedValue)
    {
        var payload = evt.Payload as string;
        Assert.NotNull(payload);
        Assert.Contains(expectedKey, payload);
        Assert.Contains(expectedValue, payload);
    }

    /// <summary>Assert collection of events matches expected sequence.</summary>
    public static void AssertEventSequence(IEnumerable<dynamic> events, params string[] expectedEventTypes)
    {
        var actual = events.Select(e => e.EventType).ToList();
        Assert.Equal(expectedEventTypes, actual);
    }

    /// <summary>Assert CQRS result has expected success state.</summary>
    public static void AssertCommandResult(dynamic result, bool shouldSucceed = true, string? errorMessage = null)
    {
        Assert.NotNull(result);

        object? successValue = null;
        bool hasProperty = false;

        try
        {
            successValue = result.IsSuccess;
            hasProperty = true;
        }
        catch (RuntimeBinderException)
        {
            try
            {
                successValue = result.Success;
                hasProperty = true;
            }
            catch (RuntimeBinderException)
            {
                // Neither property exists
            }
        }

        Assert.True(hasProperty, "Result must have either IsSuccess or Success property");

        if (shouldSucceed)
        {
            Assert.True((bool)successValue!, "Command expected to succeed");
        }
        else
        {
            Assert.False((bool)successValue!, errorMessage ?? "Command expected to fail");
        }
    }

    /// <summary>Assert query result is not null and matches predicate.</summary>
    public static void AssertQueryResult<T>(T? result, Func<T, bool>? predicate = null) where T : class
    {
        Assert.NotNull(result);

        if (predicate != null)
            Assert.True(predicate(result), "Query result does not match expected predicate");
    }

    /// <summary>Assert file metadata has expected properties.</summary>
    public static void AssertFileMetadata(
        dynamic metadata,
        string? expectedFileName = null,
        string? expectedContentType = null,
        long? minSizeBytes = null,
        long? maxSizeBytes = null)
    {
        Assert.NotNull(metadata);

        if (expectedFileName != null)
            Assert.Equal(expectedFileName, metadata.FileName);

        if (expectedContentType != null)
            Assert.Equal(expectedContentType, metadata.ContentType);

        if (minSizeBytes.HasValue)
            Assert.True(metadata.SizeBytes >= minSizeBytes.Value, $"File size {metadata.SizeBytes} < min {minSizeBytes}");

        if (maxSizeBytes.HasValue)
            Assert.True(metadata.SizeBytes <= maxSizeBytes.Value, $"File size {metadata.SizeBytes} > max {maxSizeBytes}");
    }

    /// <summary>Assert event store has recorded expected number of events.</summary>
    public static void AssertEventStoreCount(int actualCount, int expectedCount)
    {
        Assert.Equal(expectedCount, actualCount);
    }

    /// <summary>Assert saga execution completed successfully.</summary>
    public static void AssertSagaCompleted(dynamic sagaResult, string expectedAggregateId)
    {
        Assert.NotNull(sagaResult);
        Assert.Equal(expectedAggregateId, sagaResult.AggregateId);

        object? statusValue = null;
        bool hasProperty = false;

        try
        {
            statusValue = sagaResult.Status;
            hasProperty = true;
        }
        catch (RuntimeBinderException)
        {
            try
            {
                statusValue = sagaResult.State;
                hasProperty = true;
            }
            catch (RuntimeBinderException)
            {
                // Neither property exists
            }
        }

        Assert.True(hasProperty, "Saga result must have either Status or State property");
        Assert.Equal("Completed", (string)statusValue!);
    }

    /// <summary>Assert saga compensation executed on failure.</summary>
    public static void AssertSagaCompensated(dynamic sagaResult)
    {
        Assert.NotNull(sagaResult);

        object? statusValue = null;
        bool hasProperty = false;

        try
        {
            statusValue = sagaResult.Status;
            hasProperty = true;
        }
        catch (RuntimeBinderException)
        {
            try
            {
                statusValue = sagaResult.State;
                hasProperty = true;
            }
            catch (RuntimeBinderException)
            {
                // Neither property exists
            }
        }

        Assert.True(hasProperty, "Saga result must have either Status or State property");
        Assert.Equal("Compensated", (string)statusValue!);
    }
}
