using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartWorkz.Windows.Services;

/// <summary>
/// Windows implementation of system tray operations
/// </summary>
public class WindowsSystemTrayService : ISystemTrayService, IDisposable
{
    private readonly ILogger<WindowsSystemTrayService> _logger;
    private readonly Subject<DateTime> _iconClickedSubject = new();
    private string? _currentTooltip;
    private bool _isVisible;
    private bool _disposed;

    public IObservable<DateTime> IconClicked => _iconClickedSubject;

    public WindowsSystemTrayService(ILogger<WindowsSystemTrayService>? logger = null)
    {
        _logger = logger ?? new NullLogger<WindowsSystemTrayService>();
    }

    public async Task ShowIconAsync(string tooltip, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(tooltip))
        {
            throw new ArgumentException("Tooltip cannot be null or empty", nameof(tooltip));
        }

        try
        {
            // Simulate async operation
            await Task.Delay(50, cancellationToken);

            _currentTooltip = tooltip;
            _isVisible = true;
            _logger.LogInformation("Tray icon shown with tooltip: {Tooltip}", tooltip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show tray icon");
            throw;
        }
    }

    public async Task HideIconAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            // Simulate async operation
            await Task.Delay(50, cancellationToken);

            _isVisible = false;
            _currentTooltip = null;
            _logger.LogInformation("Tray icon hidden");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hide tray icon");
            throw;
        }
    }

    public async Task UpdateTooltipAsync(string tooltip, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(tooltip))
        {
            throw new ArgumentException("Tooltip cannot be null or empty", nameof(tooltip));
        }

        if (!_isVisible)
        {
            throw new InvalidOperationException("Tray icon is not visible");
        }

        try
        {
            // Simulate async operation
            await Task.Delay(25, cancellationToken);

            _currentTooltip = tooltip;
            _logger.LogInformation("Tray tooltip updated to: {Tooltip}", tooltip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tray tooltip");
            throw;
        }
    }

    /// <summary>
    /// Test helper to simulate icon click
    /// </summary>
    public void SimulateIconClick()
    {
        ThrowIfDisposed();
        _iconClickedSubject.OnNext(DateTime.UtcNow);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsSystemTrayService));
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _iconClickedSubject.Dispose();
        _disposed = true;
    }
}
