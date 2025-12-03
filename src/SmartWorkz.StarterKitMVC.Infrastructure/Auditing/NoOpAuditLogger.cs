using SmartWorkz.StarterKitMVC.Application.Abstractions;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Auditing;

public sealed class NoOpAuditLogger : IAuditLogger
{
    public Task LogAsync(string action, string? subjectId = null, string? subjectType = null, object? metadata = null, CancellationToken cancellationToken = default)
    {
        // Intentionally no-op; real implementations can plug in.
        return Task.CompletedTask;
    }
}
