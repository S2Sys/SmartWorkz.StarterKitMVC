using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartWorkz.Windows.Services;

/// <summary>
/// Windows implementation of file system watcher service
/// </summary>
public class WindowsFileSystemWatcherService : IFileSystemWatcherService, IDisposable
{
    private readonly ILogger<WindowsFileSystemWatcherService> _logger;
    private readonly Subject<FileSystemChangeEvent> _changesSubject = new();
    private FileSystemWatcher? _watcher;
    private string? _currentPath;
    private bool _disposed;

    public IObservable<FileSystemChangeEvent> FileSystemChanged => _changesSubject;
    public string? CurrentWatchPath => _currentPath;

    public WindowsFileSystemWatcherService(ILogger<WindowsFileSystemWatcherService>? logger = null)
    {
        _logger = logger ?? new NullLogger<WindowsFileSystemWatcherService>();
    }

    public async Task StartWatchingAsync(string path, string? filter = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        try
        {
            // Simulate async operation
            await Task.Delay(50, cancellationToken);

            // Clean up existing watcher if any
            _watcher?.Dispose();

            _currentPath = path;
            _watcher = new FileSystemWatcher(path)
            {
                Filter = filter ?? "*.*",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite
            };

            // Wire up event handlers
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Changed += OnChanged;
            _watcher.Renamed += OnRenamed;

            _watcher.EnableRaisingEvents = true;

            _logger.LogInformation("File system watcher started for path: {Path}, filter: {Filter}", path, filter ?? "*.*");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start file system watcher");
            throw;
        }
    }

    public async Task StopWatchingAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            await Task.Run(() =>
            {
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Dispose();
                    _watcher = null;
                    _currentPath = null;
                }
            }, cancellationToken);

            _logger.LogInformation("File system watcher stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop file system watcher");
            throw;
        }
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        var evt = new FileSystemChangeEvent(e.FullPath, FileSystemChangeType.Created, DateTime.UtcNow);
        _changesSubject.OnNext(evt);
        _logger.LogInformation("File created: {Path}", e.FullPath);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        var evt = new FileSystemChangeEvent(e.FullPath, FileSystemChangeType.Deleted, DateTime.UtcNow);
        _changesSubject.OnNext(evt);
        _logger.LogInformation("File deleted: {Path}", e.FullPath);
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        var evt = new FileSystemChangeEvent(e.FullPath, FileSystemChangeType.Modified, DateTime.UtcNow);
        _changesSubject.OnNext(evt);
        _logger.LogInformation("File modified: {Path}", e.FullPath);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        var evt = new FileSystemChangeEvent(e.FullPath, FileSystemChangeType.Renamed, DateTime.UtcNow);
        _changesSubject.OnNext(evt);
        _logger.LogInformation("File renamed from {OldPath} to {NewPath}", e.OldFullPath, e.FullPath);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsFileSystemWatcherService));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _watcher?.Dispose();
        _changesSubject.Dispose();
        _currentPath = null;
        _disposed = true;
    }
}
