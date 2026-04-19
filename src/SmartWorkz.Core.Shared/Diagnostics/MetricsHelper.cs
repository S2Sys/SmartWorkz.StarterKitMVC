namespace SmartWorkz.Core.Shared.Diagnostics;

using System.Diagnostics;

/// <summary>
/// Provides utilities for collecting and tracking performance metrics.
/// </summary>
public static class MetricsHelper
{
    /// <summary>
    /// Starts a timer and returns an IDisposable that logs elapsed time on disposal.
    /// </summary>
    public static IDisposable StartTimer(string? name = null)
    {
        return new TimerScope(name);
    }

    /// <summary>
    /// Tracks the execution time and result of a function.
    /// </summary>
    public static (T result, TimeSpan elapsed) TrackExecution<T>(Func<T> action, string name)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = action();
            stopwatch.Stop();
            return (result, stopwatch.Elapsed);
        }
        catch
        {
            stopwatch.Stop();
            throw;
        }
    }

    /// <summary>
    /// Captures memory usage before and after a block of code execution.
    /// </summary>
    public static MemorySnapshot MeasureMemory(Action action)
    {
        var beforeGC = GC.GetTotalMemory(false);
        var before = Process.GetCurrentProcess().WorkingSet64;

        action();

        var after = Process.GetCurrentProcess().WorkingSet64;
        var afterGC = GC.GetTotalMemory(false);

        return new MemorySnapshot(
            before,
            after,
            after - before,
            beforeGC,
            afterGC,
            afterGC - beforeGC);
    }

    /// <summary>
    /// Represents a snapshot of memory usage.
    /// </summary>
    public class MemorySnapshot
    {
        public MemorySnapshot(
            long beforeWorkingSet,
            long afterWorkingSet,
            long workingSetChange,
            long beforeGC,
            long afterGC,
            long gcChange)
        {
            BeforeWorkingSet = beforeWorkingSet;
            AfterWorkingSet = afterWorkingSet;
            WorkingSetChange = workingSetChange;
            BeforeGC = beforeGC;
            AfterGC = afterGC;
            GCChange = gcChange;
        }

        public long BeforeWorkingSet { get; }
        public long AfterWorkingSet { get; }
        public long WorkingSetChange { get; }
        public long BeforeGC { get; }
        public long AfterGC { get; }
        public long GCChange { get; }

        public override string ToString()
            => $"WorkingSet: {WorkingSetChange} bytes | GC: {GCChange} bytes";
    }

    /// <summary>
    /// Tracks HTTP request metrics.
    /// </summary>
    public class RequestMetrics
    {
        public long TotalRequests { get; set; }
        public long TotalErrors { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan AverageDuration => TotalRequests > 0
            ? TimeSpan.FromMilliseconds(TotalDuration.TotalMilliseconds / TotalRequests)
            : TimeSpan.Zero;

        public void RecordRequest(TimeSpan duration, bool isError = false)
        {
            TotalRequests++;
            TotalDuration = TotalDuration.Add(duration);
            if (isError)
                TotalErrors++;
        }

        public double ErrorRate => TotalRequests > 0
            ? (double)TotalErrors / TotalRequests * 100
            : 0;

        public override string ToString()
            => $"Requests: {TotalRequests} | Errors: {TotalErrors} ({ErrorRate:F2}%) | Avg Duration: {AverageDuration.TotalMilliseconds:F2}ms";
    }

    /// <summary>
    /// Aggregates multiple metrics for collection and reporting.
    /// </summary>
    public class MetricsCollector
    {
        private readonly Dictionary<string, RequestMetrics> _metrics = [];

        public RequestMetrics GetOrCreate(string name)
        {
            if (!_metrics.ContainsKey(name))
                _metrics[name] = new RequestMetrics();
            return _metrics[name];
        }

        public void RecordRequest(string name, TimeSpan duration, bool isError = false)
        {
            var metrics = GetOrCreate(name);
            metrics.RecordRequest(duration, isError);
        }

        public RequestMetrics? Get(string name)
            => _metrics.TryGetValue(name, out var metrics) ? metrics : null;

        public Dictionary<string, RequestMetrics> GetAll()
            => new(_metrics);

        public override string ToString()
        {
            if (_metrics.Count == 0)
                return "No metrics collected";

            var lines = _metrics.Select(kvp => $"{kvp.Key}: {kvp.Value}");
            return string.Join(Environment.NewLine, lines);
        }
    }

    /// <summary>
    /// Disposable scope for timing execution with automatic logging.
    /// </summary>
    private sealed class TimerScope : IDisposable
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly string? _name;
        private bool _disposed;

        public TimerScope(string? name = null)
        {
            _name = name;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _stopwatch.Stop();
            var message = _name != null
                ? $"{_name}: {_stopwatch.ElapsedMilliseconds}ms"
                : $"Elapsed: {_stopwatch.ElapsedMilliseconds}ms";
            Debug.WriteLine(message);
            _disposed = true;
        }
    }
}
