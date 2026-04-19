using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.Core.Shared.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attr = field?.GetCustomAttribute<DisplayAttribute>();
        return attr?.GetName() ?? value.ToString();
    }
}
