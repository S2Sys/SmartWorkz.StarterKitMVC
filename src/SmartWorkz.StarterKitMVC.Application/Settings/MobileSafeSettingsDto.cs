namespace SmartWorkz.StarterKitMVC.Application.Settings;

public sealed record MobileSafeSettingsDto(
    string ThemeMode,
    string Language,
    bool NotificationsEnabled,
    bool OfflineModeEnabled,
    int SyncIntervalMinutes,
    bool DataSaverEnabled
);
