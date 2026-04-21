namespace SmartWorkz.Core.Tests.Diagnostics;

using System.Diagnostics;
using SmartWorkz.Shared.Diagnostics;

public class MetricsHelperTests
{
    #region StartTimer Tests

    [Fact]
    public void StartTimer_CreatesDisposableScope()
    {
        // Act
        var timer = MetricsHelper.StartTimer("test");

        // Assert
        Assert.NotNull(timer);
        Assert.IsAssignableFrom<IDisposable>(timer);

        // Cleanup
        timer.Dispose();
    }

    [Fact]
    public void StartTimer_WithName_DoesNotThrow()
    {
        // Act
        using var timer = MetricsHelper.StartTimer("OperationName");

        // Assert - no exception thrown
        Assert.NotNull(timer);
    }

    [Fact]
    public void StartTimer_WithoutName_DoesNotThrow()
    {
        // Act
        using var timer = MetricsHelper.StartTimer();

        // Assert - no exception thrown
        Assert.NotNull(timer);
    }

    [Fact]
    public void StartTimer_CanBeDisposed()
    {
        // Act
        var timer = MetricsHelper.StartTimer();
        timer.Dispose();

        // Assert - no exception thrown
        Assert.True(true);
    }

    #endregion

    #region TrackExecution Tests

    [Fact]
    public void TrackExecution_WithAction_ReturnsResultAndElapsed()
    {
        // Arrange
        Func<string> action = () => "test result";

        // Act
        var (result, elapsed) = MetricsHelper.TrackExecution(action, "test");

        // Assert
        Assert.Equal("test result", result);
        Assert.True(elapsed.TotalMilliseconds >= 0);
    }

    [Fact]
    public void TrackExecution_WithIntegerResult_TracksCorrectly()
    {
        // Arrange
        Func<int> action = () => 42;

        // Act
        var (result, elapsed) = MetricsHelper.TrackExecution(action, "integer test");

        // Assert
        Assert.Equal(42, result);
        Assert.True(elapsed >= TimeSpan.Zero);
    }

    [Fact]
    public void TrackExecution_MeasuresElapsedTime()
    {
        // Arrange
        Func<string> action = () =>
        {
            System.Threading.Thread.Sleep(50);
            return "done";
        };

        // Act
        var (_, elapsed) = MetricsHelper.TrackExecution(action, "slow operation");

        // Assert
        Assert.True(elapsed.TotalMilliseconds >= 50);
    }

    [Fact]
    public void TrackExecution_WithException_ReThrowsAndStopsTimer()
    {
        // Arrange
        Func<string> action = () => throw new InvalidOperationException("test error");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => MetricsHelper.TrackExecution(action, "failing"));
    }

    #endregion

    #region MeasureMemory Tests

    [Fact]
    public void MeasureMemory_ReturnsSnapshot()
    {
        // Arrange
        Action action = () => { };

        // Act
        var snapshot = MetricsHelper.MeasureMemory(action);

        // Assert
        Assert.NotNull(snapshot);
        Assert.True(snapshot.BeforeWorkingSet >= 0);
        Assert.True(snapshot.AfterWorkingSet >= 0);
    }

    [Fact]
    public void MeasureMemory_TracksWorkingSetChange()
    {
        // Arrange
        Action action = () =>
        {
            var data = new byte[1000000]; // Allocate 1MB
            GC.KeepAlive(data);
        };

        // Act
        var snapshot = MetricsHelper.MeasureMemory(action);

        // Assert
        Assert.NotNull(snapshot);
        Assert.True(snapshot.WorkingSetChange >= 0);
    }

    [Fact]
    public void MeasureMemory_HasToStringOverride()
    {
        // Arrange
        var snapshot = new MetricsHelper.MemorySnapshot(
            beforeWorkingSet: 1000000,
            afterWorkingSet: 2000000,
            workingSetChange: 1000000,
            beforeGC: 500000,
            afterGC: 600000,
            gcChange: 100000);

        // Act
        var output = snapshot.ToString();

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("WorkingSet", output);
        Assert.Contains("GC", output);
    }

    #endregion

    #region RequestMetrics Tests

    [Fact]
    public void RequestMetrics_InitializesWithZero()
    {
        // Arrange
        var metrics = new MetricsHelper.RequestMetrics();

        // Assert
        Assert.Equal(0, metrics.TotalRequests);
        Assert.Equal(0, metrics.TotalErrors);
        Assert.Equal(TimeSpan.Zero, metrics.TotalDuration);
    }

    [Fact]
    public void RequestMetrics_RecordRequest_IncrementsTotal()
    {
        // Arrange
        var metrics = new MetricsHelper.RequestMetrics();

        // Act
        metrics.RecordRequest(TimeSpan.FromMilliseconds(100));

        // Assert
        Assert.Equal(1, metrics.TotalRequests);
        Assert.Equal(0, metrics.TotalErrors);
    }

    [Fact]
    public void RequestMetrics_RecordRequest_AggregatesDuration()
    {
        // Arrange
        var metrics = new MetricsHelper.RequestMetrics();

        // Act
        metrics.RecordRequest(TimeSpan.FromMilliseconds(100));
        metrics.RecordRequest(TimeSpan.FromMilliseconds(200));

        // Assert
        Assert.Equal(2, metrics.TotalRequests);
        Assert.Equal(TimeSpan.FromMilliseconds(300), metrics.TotalDuration);
    }

    [Fact]
    public void RequestMetrics_RecordRequest_WithError_IncrementsErrorCount()
    {
        // Arrange
        var metrics = new MetricsHelper.RequestMetrics();

        // Act
        metrics.RecordRequest(TimeSpan.FromMilliseconds(100), isError: false);
        metrics.RecordRequest(TimeSpan.FromMilliseconds(100), isError: true);

        // Assert
        Assert.Equal(2, metrics.TotalRequests);
        Assert.Equal(1, metrics.TotalErrors);
    }

    [Fact]
    public void RequestMetrics_AverageDuration_CalculatesCorrectly()
    {
        // Arrange
        var metrics = new MetricsHelper.RequestMetrics();

        // Act
        metrics.RecordRequest(TimeSpan.FromMilliseconds(100));
        metrics.RecordRequest(TimeSpan.FromMilliseconds(200));
        metrics.RecordRequest(TimeSpan.FromMilliseconds(300));

        // Assert
        Assert.Equal(TimeSpan.FromMilliseconds(200), metrics.AverageDuration);
    }

    [Fact]
    public void RequestMetrics_AverageDuration_WithZeroRequests_ReturnsZero()
    {
        // Arrange
        var metrics = new MetricsHelper.RequestMetrics();

        // Act
        var average = metrics.AverageDuration;

        // Assert
        Assert.Equal(TimeSpan.Zero, average);
    }

    [Fact]
    public void RequestMetrics_ErrorRate_CalculatesCorrectly()
    {
        // Arrange
        var metrics = new MetricsHelper.RequestMetrics();

        // Act
        for (int i = 0; i < 10; i++)
            metrics.RecordRequest(TimeSpan.FromMilliseconds(100), isError: i % 2 == 0);

        // Assert
        Assert.Equal(50, metrics.ErrorRate); // 5 errors out of 10 requests
    }

    [Fact]
    public void RequestMetrics_ErrorRate_WithZeroRequests_ReturnsZero()
    {
        // Arrange
        var metrics = new MetricsHelper.RequestMetrics();

        // Act
        var errorRate = metrics.ErrorRate;

        // Assert
        Assert.Equal(0, errorRate);
    }

    [Fact]
    public void RequestMetrics_ToString_ReturnsFormattedString()
    {
        // Arrange
        var metrics = new MetricsHelper.RequestMetrics();
        metrics.RecordRequest(TimeSpan.FromMilliseconds(100));
        metrics.RecordRequest(TimeSpan.FromMilliseconds(100), isError: true);

        // Act
        var output = metrics.ToString();

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("Requests", output);
        Assert.Contains("Errors", output);
    }

    #endregion

    #region MetricsCollector Tests

    [Fact]
    public void MetricsCollector_GetOrCreate_CreatesNewMetrics()
    {
        // Arrange
        var collector = new MetricsHelper.MetricsCollector();

        // Act
        var metrics = collector.GetOrCreate("operation1");

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(0, metrics.TotalRequests);
    }

    [Fact]
    public void MetricsCollector_GetOrCreate_ReturnsSameInstance()
    {
        // Arrange
        var collector = new MetricsHelper.MetricsCollector();

        // Act
        var metrics1 = collector.GetOrCreate("operation1");
        var metrics2 = collector.GetOrCreate("operation1");

        // Assert
        Assert.Same(metrics1, metrics2);
    }

    [Fact]
    public void MetricsCollector_RecordRequest_TracksMetrics()
    {
        // Arrange
        var collector = new MetricsHelper.MetricsCollector();

        // Act
        collector.RecordRequest("api_call", TimeSpan.FromMilliseconds(100));
        collector.RecordRequest("api_call", TimeSpan.FromMilliseconds(200));

        // Assert
        var metrics = collector.Get("api_call");
        Assert.NotNull(metrics);
        Assert.Equal(2, metrics.TotalRequests);
    }

    [Fact]
    public void MetricsCollector_Get_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var collector = new MetricsHelper.MetricsCollector();

        // Act
        var metrics = collector.Get("nonexistent");

        // Assert
        Assert.Null(metrics);
    }

    [Fact]
    public void MetricsCollector_GetAll_ReturnsAllMetrics()
    {
        // Arrange
        var collector = new MetricsHelper.MetricsCollector();
        collector.RecordRequest("op1", TimeSpan.FromMilliseconds(100));
        collector.RecordRequest("op2", TimeSpan.FromMilliseconds(200));

        // Act
        var all = collector.GetAll();

        // Assert
        Assert.Equal(2, all.Count);
        Assert.True(all.ContainsKey("op1"));
        Assert.True(all.ContainsKey("op2"));
    }

    [Fact]
    public void MetricsCollector_ToString_WithMetrics_ReturnsFormatted()
    {
        // Arrange
        var collector = new MetricsHelper.MetricsCollector();
        collector.RecordRequest("operation", TimeSpan.FromMilliseconds(100));

        // Act
        var output = collector.ToString();

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("operation", output);
    }

    [Fact]
    public void MetricsCollector_ToString_WithoutMetrics_ReturnsEmpty()
    {
        // Arrange
        var collector = new MetricsHelper.MetricsCollector();

        // Act
        var output = collector.ToString();

        // Assert
        Assert.Contains("No metrics", output);
    }

    #endregion
}
