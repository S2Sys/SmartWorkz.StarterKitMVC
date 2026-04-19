namespace SmartWorkz.Core.Shared.Extensions;

public static class GuidExtensions
{
    public static bool IsEmpty(this Guid value) => value == Guid.Empty;
    public static bool IsNotEmpty(this Guid value) => value != Guid.Empty;
    public static Guid IfEmpty(this Guid value, Guid defaultValue) => value.IsEmpty() ? defaultValue : value;
    public static string ToShortString(this Guid value) => value.ToString("N").Substring(0, 8);
    public static bool TryParseExact(string? value, out Guid result)
    {
        result = Guid.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;
        return Guid.TryParse(value, out result);
    }
}
