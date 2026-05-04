#if __MACCATALYST__
namespace SmartWorkz.Mobile;

using SmartWorkz.Shared;
using System.Threading;
using System.Threading.Tasks;

public sealed partial class GeofencingService
{
    private partial async Task<Result<bool>> StartMonitoringAsyncPlatform(GeofenceRegion region, CancellationToken ct)
    {
        await Task.CompletedTask;
        return Result.Fail<bool>(new Error("GEOFENCING.NOT_SUPPORTED",
            "Geofencing is not supported on macOS"));
    }

    private partial async Task<Result<bool>> StopMonitoringAsyncPlatform(string regionId, CancellationToken ct)
    {
        await Task.CompletedTask;
        return Result.Fail<bool>(new Error("GEOFENCING.NOT_SUPPORTED",
            "Geofencing is not supported on macOS"));
    }

    private partial Task<bool> IsAvailableAsyncPlatform(CancellationToken ct) =>
        Task.FromResult(false);
}
#endif
