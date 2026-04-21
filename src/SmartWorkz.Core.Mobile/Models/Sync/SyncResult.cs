namespace SmartWorkz.Core.Mobile;

public sealed class SyncResult
{
    public int SyncedCount { get; init; }
    public int FailedCount { get; init; }
    public int ConflictCount { get; init; }
    public IReadOnlyList<Guid> FailedOperationIds { get; init; } = [];
    public DateTime SyncedAt { get; init; } = DateTime.UtcNow;
}
