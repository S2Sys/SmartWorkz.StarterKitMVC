using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SmartWorkz.Windows.Services;

/// <summary>
/// Windows implementation of taskbar operations
/// </summary>
public class WindowsTaskbarService : ITaskbarService, IDisposable
{
    private readonly ILogger<WindowsTaskbarService> _logger;
    private bool _disposed;

    public WindowsTaskbarService(ILogger<WindowsTaskbarService>? logger = null)
    {
        _logger = logger ?? new NullLogger<WindowsTaskbarService>();
    }

    public async Task SetProgressAsync(int current, int total, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (current < 0 || total <= 0 || current > total)
        {
            throw new ArgumentException("Progress values must be valid: 0 <= current <= total and total > 0");
        }

        try
        {
            // Simulate async operation
            await Task.Delay(25, cancellationToken);

            var percentage = total > 0 ? (current * 100) / total : 0;
            _logger.LogInformation("Taskbar progress set to {Percentage}%", percentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set taskbar progress");
            throw;
        }
    }

    public async Task SetBadgeAsync(string? badge, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            // Simulate async operation
            await Task.Delay(25, cancellationToken);

            if (badge == null)
            {
                _logger.LogInformation("Taskbar badge cleared");
            }
            else
            {
                _logger.LogInformation("Taskbar badge set to {Badge}", badge);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set taskbar badge");
            throw;
        }
    }

    public async Task ActivateWindowAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            // Simulate async operation
            await Task.Delay(50, cancellationToken);
            _logger.LogInformation("Window activated from taskbar");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate window");
            throw;
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsTaskbarService));
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
