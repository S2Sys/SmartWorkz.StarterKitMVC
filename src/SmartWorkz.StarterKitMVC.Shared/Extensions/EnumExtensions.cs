using System.ComponentModel;
using System.Reflection;

namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="Enum"/> types.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description attribute value of an enum member.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>The description if available; otherwise the enum name.</returns>
    /// <example>
    /// <code>
    /// public enum Status
    /// {
    ///     [Description("Currently Active")]
    ///     Active,
    ///     [Description("Temporarily Inactive")]
    ///     Inactive
    /// }
    /// 
    /// var status = Status.Active;
    /// var desc = status.GetDescription(); // "Currently Active"
    /// </code>
    /// </example>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}
