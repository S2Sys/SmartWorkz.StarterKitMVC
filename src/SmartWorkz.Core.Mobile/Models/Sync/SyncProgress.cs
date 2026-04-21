namespace SmartWorkz.Core.Mobile;

public sealed class SyncProgress
{
    public int Current { get; init; }
    public int Total { get; init; }
    public string StatusMessage { get; init; } = "";
    public bool IsComplete { get; init; }
}
