namespace SmartWorkz.Core.Shared.Utilities;

using System.ComponentModel;
using System.Reflection;

/// <summary>
/// Provides utilities for enum operations including reflection and description retrieval.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Gets the description of an enum value from its [Description] attribute.
    /// Falls back to the enum name if no description is found.
    /// </summary>
    public static string GetDescription(Enum value)
    {
        try
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
                return value.ToString();

            var descriptionAttribute = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute?.Description ?? value.ToString();
        }
        catch
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// Attempts to get an enum value by its name.
    /// </summary>
    public static Result<T> GetValue<T>(string name) where T : Enum
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Fail<T>("Error.InvalidEnumName", "Enum name cannot be null or empty");

            if (!Enum.TryParse(typeof(T), name, ignoreCase: true, out var result) || result == null)
                return Result.Fail<T>("Error.EnumNotFound", $"Enum value '{name}' not found in {typeof(T).Name}");

            return Result.Ok((T)result);
        }
        catch (Exception ex)
        {
            return Result.Fail<T>("Error.GetEnumValue", ex.Message);
        }
    }

    /// <summary>
    /// Returns all values of the specified enum type as a list.
    /// </summary>
    public static List<T> GetAllValues<T>() where T : Enum
    {
        try
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Gets the name of an enum value.
    /// </summary>
    public static string GetName(Enum value)
    {
        try
        {
            return Enum.GetName(value.GetType(), value) ?? value.ToString();
        }
        catch
        {
            return value.ToString();
        }
    }
}
