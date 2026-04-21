namespace SmartWorkz.Core.Mobile;

public interface ISyncService
{
    Task<Result<SyncResult>> SyncAsync(CancellationToken ct = default);
    Task<Result> QueueOperationAsync(SyncOperation operation, CancellationToken ct = default);
    Task<Result<IEnumerable<SyncOperation>>> GetPendingOperationsAsync(CancellationToken ct = default);
    void SetConflictResolutionStrategy(ConflictStrategy strategy);
    IObservable<SyncProgress> OnSyncProgress();
}
