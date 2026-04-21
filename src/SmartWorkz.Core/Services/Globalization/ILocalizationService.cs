namespace SmartWorkz.Core;

public interface ILocalizationService
{
    string GetString(string key, string? defaultValue = null);
    string GetString(string key, params object[] args);
    Dictionary<string, string> GetStrings(string prefix);
    Task<string> GetStringAsync(string key, string? defaultValue = null, CancellationToken cancellationToken = default);
}
