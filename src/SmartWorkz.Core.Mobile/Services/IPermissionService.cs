namespace SmartWorkz.Core.Mobile;

public interface IPermissionService
{
    Task<PermissionStatus> CheckAsync(MobilePermission permission, CancellationToken ct = default);
    Task<PermissionStatus> RequestAsync(MobilePermission permission, CancellationToken ct = default);
    Task<Dictionary<MobilePermission, PermissionStatus>> RequestMultipleAsync(CancellationToken ct = default, params MobilePermission[] permissions);
}
