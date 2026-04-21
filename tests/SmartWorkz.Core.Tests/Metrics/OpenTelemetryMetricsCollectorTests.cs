namespace SmartWorkz.Core.Tests.Metrics;

using SmartWorkz.Core.Shared.Metrics;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

public class OpenTelemetryMetricsCollectorTests
{
    private readonly Mock<ILogger<OpenTelemetryMetricsCollector>> _mockLogger;
    private readonly OpenTelemetryMetricsCollector _collector;

    public OpenTelemetryMetricsCollectorTests()
    {
        _mockLogger = new Mock<ILogger<OpenTelemetryMetricsCollector>>();
        _collector = new OpenTelemetryMetricsCollector(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new OpenTelemetryMetricsCollector(null!));
    }

    [Fact]
    public void RecordOperationDuration_WithValidData_Records()
    {
        _collector.RecordOperationDuration("TestOp", 150, "success");

        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void RecordOperationDuration_WithNullOperationName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _collector.RecordOperationDuration(null!, 100));
    }

    [Fact]
    public void RecordOperationDuration_WithEmptyOperationName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _collector.RecordOperationDuration("", 100));
    }

    [Fact]
    public void RecordOperationCount_WithValidData_Increments()
    {
        _collector.RecordOperationCount("TestCounter", 5, "success");
        // No exception thrown - metric recorded internally
    }

    [Fact]
    public void RecordOperationCount_WithNullOperationName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _collector.RecordOperationCount(null!, 5));
    }

    [Fact]
    public void RecordError_WithValidException_RecordsError()
    {
        var ex = new InvalidOperationException("Test error");

        _collector.RecordError("TestOp", ex);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            ex,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void RecordError_WithNullException_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _collector.RecordError("TestOp", null!));
    }

    [Fact]
    public void IncrementCounter_WithValidName_Increments()
    {
        _collector.IncrementCounter("MyCounter", 3);
        // No exception thrown
    }

    [Fact]
    public void IncrementCounter_WithNullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _collector.IncrementCounter(null!, 3));
    }

    [Fact]
    public void RecordGaugeValue_WithValidData_Records()
    {
        _collector.RecordGaugeValue("QueueDepth", 42.5);
        // No exception thrown
    }

    [Fact]
    public void RecordGaugeValue_WithNullMetricName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _collector.RecordGaugeValue(null!, 42.5));
    }
}
