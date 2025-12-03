using SmartWorkz.StarterKitMVC.Domain.Settings;

namespace SmartWorkz.StarterKitMVC.Application.Settings;

public interface ISettingsCache
{
    Task<SettingValue?> GetAsync(string cacheKey, CancellationToken ct = default);
    Task SetAsync(string cacheKey, SettingValue value, CancellationToken ct = default);
    Task InvalidateAsync(string cacheKeyPrefix, CancellationToken ct = default);
}
