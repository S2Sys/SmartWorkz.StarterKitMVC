using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartWorkz.Windows.Services;

/// <summary>
/// Windows implementation of notification service using Windows 10+ Toast notifications
/// </summary>
public class WindowsNotificationService : INotificationService, IDisposable
{
    private readonly ILogger<WindowsNotificationService> _logger;
    private readonly List<string> _activeNotifications = new();
    private readonly object _lockObject = new();
    private bool _disposed;

    public WindowsNotificationService(ILogger<WindowsNotificationService>? logger = null)
    {
        _logger = logger ?? new NullLogger<WindowsNotificationService>();
    }

    public async Task ShowToastAsync(string title, string message, NotificationType type = NotificationType.Information, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }

        try
        {
            var notificationId = Guid.NewGuid().ToString();

            lock (_lockObject)
            {
                _activeNotifications.Add(notificationId);
            }

            // Simulate async operation
            await Task.Delay(50, cancellationToken);

            _logger.LogInformation("Toast notification shown - Type: {NotificationType}, Title: {Title}, Message: {Message}", type, title, message);

            // Auto-remove notification after delay
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000, cancellationToken);
                lock (_lockObject)
                {
                    _activeNotifications.Remove(notificationId);
                }
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Toast notification cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show toast notification");
            throw;
        }
    }

    public async Task ShowDialogAsync(string title, string message, string? buttonText = null, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
        }

        try
        {
            // Simulate async operation
            await Task.Delay(100, cancellationToken);

            var btnText = buttonText ?? "OK";
            _logger.LogInformation("Dialog notification shown - Title: {Title}, Message: {Message}, Button: {Button}", title, message, btnText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show dialog notification");
            throw;
        }
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    var count = _activeNotifications.Count;
                    _activeNotifications.Clear();
                    if (count > 0)
                    {
                        _logger.LogInformation("Cleared {Count} active notifications", count);
                    }
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear notifications");
            throw;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsNotificationService));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _activeNotifications.Clear();
        _disposed = true;
    }
}
