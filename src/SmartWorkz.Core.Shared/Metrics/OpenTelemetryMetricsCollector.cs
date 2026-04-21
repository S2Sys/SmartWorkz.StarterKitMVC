namespace SmartWorkz.Shared;

using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

/// <summary>
/// OpenTelemetry-based implementation of IMetricsCollector.
/// Collects metrics using System.Diagnostics.Metrics for export to Prometheus/Grafana.
/// </summary>
public class OpenTelemetryMetricsCollector : IMetricsCollector
{
    private readonly Meter _meter;
    private readonly Histogram<long> _operationDurationHistogram;
    private readonly Counter<int> _operationCountCounter;
    private readonly Histogram<double> _gaugeHistogram;
    private readonly Counter<int> _errorCounter;
    private readonly ILogger<OpenTelemetryMetricsCollector> _logger;

    public OpenTelemetryMetricsCollector(ILogger<OpenTelemetryMetricsCollector> logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
        _meter = new Meter("SmartWorkz.Metrics", "1.0.0");

        _operationDurationHistogram = _meter.CreateHistogram<long>(
            "smartworkz.operation.duration_ms",
            unit: "ms",
            description: "Duration of operations in milliseconds");

        _operationCountCounter = _meter.CreateCounter<int>(
            "smartworkz.operation.count",
            description: "Total count of operations");

        _gaugeHistogram = _meter.CreateHistogram<double>(
            "smartworkz.gauge",
            description: "Gauge metric values");

        _errorCounter = _meter.CreateCounter<int>(
            "smartworkz.errors",
            description: "Total count of errors");
    }

    public void RecordOperationDuration(string operationName, long durationMs, string? status = null, Dictionary<string, object>? tags = null)
    {
        Guard.NotEmpty(operationName, nameof(operationName));

        var tagsArray = BuildTagsArray(operationName, status, tags);
        _operationDurationHistogram.Record(durationMs, tagsArray);
        _logger.LogInformation("Operation {Operation} completed in {Duration}ms", operationName, durationMs);
    }

    public void RecordOperationCount(string operationName, int count = 1, string? status = null, Dictionary<string, object>? tags = null)
    {
        Guard.NotEmpty(operationName, nameof(operationName));

        var tagsArray = BuildTagsArray(operationName, status, tags);
        _operationCountCounter.Add(count, tagsArray);
    }

    public void RecordGaugeValue(string metricName, double value, Dictionary<string, object>? tags = null)
    {
        Guard.NotEmpty(metricName, nameof(metricName));

        var tagsArray = BuildTagsArray(metricName, null, tags);
        _gaugeHistogram.Record(value, tagsArray);
    }

    public void RecordError(string operationName, Exception ex, Dictionary<string, object>? tags = null)
    {
        Guard.NotEmpty(operationName, nameof(operationName));
        Guard.NotNull(ex, nameof(ex));

        var tagsArray = BuildTagsArray(operationName, "error", tags);
        _errorCounter.Add(1, tagsArray);
        _logger.LogError(ex, "Error in operation {Operation}", operationName);
    }

    public void IncrementCounter(string counterName, int increment = 1, Dictionary<string, object>? tags = null)
    {
        Guard.NotEmpty(counterName, nameof(counterName));

        var tagsArray = BuildTagsArray(counterName, null, tags);
        _operationCountCounter.Add(increment, tagsArray);
    }

    private static KeyValuePair<string, object?>[] BuildTagsArray(string name, string? status, Dictionary<string, object>? tags)
    {
        var tagsList = new List<KeyValuePair<string, object?>>
        {
            new("operation", name)
        };

        if (!string.IsNullOrEmpty(status))
            tagsList.Add(new("status", status));

        if (tags != null)
        {
            tagsList.AddRange(tags.Select(t => new KeyValuePair<string, object?>(t.Key, t.Value)));
        }

        return tagsList.ToArray();
    }
}
