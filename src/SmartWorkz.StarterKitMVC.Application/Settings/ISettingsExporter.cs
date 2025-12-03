namespace SmartWorkz.StarterKitMVC.Application.Settings;

public interface ISettingsExporter
{
    Task<string> ExportAsync(string? tenantId = null, CancellationToken ct = default);
    Task ImportAsync(string json, string? tenantId = null, CancellationToken ct = default);
}
