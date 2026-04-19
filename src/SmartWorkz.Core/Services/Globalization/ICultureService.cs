namespace SmartWorkz.Core.Services.Globalization;

public interface ICultureService
{
    string CurrentCulture { get; }
    string CurrentUICulture { get; }
    IEnumerable<string> SupportedCultures { get; }
    void SetCulture(string culture);
    void SetUICulture(string culture);
}
