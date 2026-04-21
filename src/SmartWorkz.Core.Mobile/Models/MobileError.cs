namespace SmartWorkz.Core.Mobile;

public sealed class MobileError
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? StackTrace { get; init; }
    public required string Platform { get; init; }
    public required string DeviceId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Context { get; init; }
}
