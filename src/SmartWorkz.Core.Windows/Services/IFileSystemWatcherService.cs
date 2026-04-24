namespace SmartWorkz.Windows.Services;

/// <summary>
/// Interface for file system monitoring
/// </summary>
public interface IFileSystemWatcherService
{
    /// <summary>
    /// Observable for file system change events
    /// </summary>
    IObservable<FileSystemChangeEvent> FileSystemChanged { get; }

    /// <summary>
    /// Start watching a directory for changes
    /// </summary>
    Task StartWatchingAsync(string path, string? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop watching for changes
    /// </summary>
    Task StopWatchingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the current watched path
    /// </summary>
    string? CurrentWatchPath { get; }
}

/// <summary>
/// Represents a file system change event
/// </summary>
public record FileSystemChangeEvent(
    string Path,
    FileSystemChangeType ChangeType,
    DateTime Timestamp);

/// <summary>
/// Type of file system change
/// </summary>
public enum FileSystemChangeType
{
    Created,
    Deleted,
    Modified,
    Renamed
}
