namespace SmartWorkz.Core.Shared.Metrics;

/// <summary>
/// Abstraction for collecting application metrics and performance data.
/// Enables tracking of operation duration, throughput, error rates, and custom metrics.
/// Implementations integrate with OpenTelemetry for export to Prometheus/Grafana.
/// </summary>
public interface IMetricsCollector
{
    /// <summary>
    /// Record operation duration in milliseconds.
    /// </summary>
    /// <param name="operationName">Name of the operation being measured.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    /// <param name="status">Optional status (e.g., "success", "error").</param>
    /// <param name="tags">Optional metadata tags for grouping and filtering.</param>
    void RecordOperationDuration(string operationName, long durationMs, string? status = null, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Record operation count (increments counter).
    /// </summary>
    /// <param name="operationName">Name of the operation.</param>
    /// <param name="count">Number to increment by (default 1).</param>
    /// <param name="status">Optional status label.</param>
    /// <param name="tags">Optional metadata tags.</param>
    void RecordOperationCount(string operationName, int count = 1, string? status = null, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Record a gauge value (e.g., queue depth, memory usage).
    /// </summary>
    /// <param name="metricName">Name of the gauge metric.</param>
    /// <param name="value">The gauge value to record.</param>
    /// <param name="tags">Optional metadata tags.</param>
    void RecordGaugeValue(string metricName, double value, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Record error/exception occurrence.
    /// </summary>
    /// <param name="operationName">Name of the operation that failed.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="tags">Optional metadata tags.</param>
    void RecordError(string operationName, Exception ex, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Increment a custom counter.
    /// </summary>
    /// <param name="counterName">Name of the counter.</param>
    /// <param name="increment">Amount to increment (default 1).</param>
    /// <param name="tags">Optional metadata tags.</param>
    void IncrementCounter(string counterName, int increment = 1, Dictionary<string, object>? tags = null);
}
