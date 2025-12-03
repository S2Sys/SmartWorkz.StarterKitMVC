namespace SmartWorkz.StarterKitMVC.Application.Abstractions;

public interface IAuditLogger
{
    Task LogAsync(string action, string? subjectId = null, string? subjectType = null, object? metadata = null, CancellationToken cancellationToken = default);
}
