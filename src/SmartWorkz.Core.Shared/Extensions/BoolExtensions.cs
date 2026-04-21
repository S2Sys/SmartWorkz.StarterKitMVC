namespace SmartWorkz.Shared;

public static class BoolExtensions
{
    public static string ToYesNo(this bool value) => value ? "Yes" : "No";
    public static string ToOnOff(this bool value) => value ? "On" : "Off";
    public static string ToEnabledDisabled(this bool value) => value ? "Enabled" : "Disabled";
    public static int ToInt(this bool value) => value ? 1 : 0;
    public static T IfTrue<T>(this bool condition, T trueValue, T falseValue)
        => condition ? trueValue : falseValue;
}
