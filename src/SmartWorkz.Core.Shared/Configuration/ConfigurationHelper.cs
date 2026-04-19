using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace SmartWorkz.Core.Shared.Configuration;

/// <summary>
/// Provides typed access to configuration values with automatic type conversion and validation.
///
/// This sealed class implements IConfigurationHelper to provide a strongly-typed interface
/// for accessing configuration values. It supports automatic type conversion for common types
/// including strings, numeric types, booleans, DateTimes, and enums.
/// </summary>
public sealed class ConfigurationHelper : IConfigurationHelper
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the ConfigurationHelper class.
    /// </summary>
    /// <param name="configuration">The configuration source to read from.</param>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public ConfigurationHelper(IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");

        _configuration = configuration;
    }

    /// <summary>
    /// Gets a required configuration value and converts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the configuration value to.</typeparam>
    /// <param name="key">The configuration key to retrieve.</param>
    /// <returns>The configuration value converted to type T.</returns>
    /// <exception cref="ConfigurationValidationException">
    /// Thrown when the configuration key is not found, is empty, or cannot be converted to type T.
    /// </exception>
    public T GetRequired<T>(string key)
    {
        var value = _configuration[key];

        if (string.IsNullOrWhiteSpace(value))
            throw new ConfigurationValidationException($"Required configuration key '{key}' not found or empty.");

        try
        {
            return ConvertValue<T>(value);
        }
        catch (Exception ex) when (!(ex is ConfigurationValidationException))
        {
            throw new ConfigurationValidationException($"Failed to convert configuration key '{key}' to type '{typeof(T).Name}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets an optional configuration value and converts it to the specified type,
    /// returning a default value if the key is not found or conversion fails.
    /// </summary>
    /// <typeparam name="T">The type to convert the configuration value to.</typeparam>
    /// <param name="key">The configuration key to retrieve.</param>
    /// <param name="defaultValue">The value to return if the key is not found or conversion fails.</param>
    /// <returns>The configuration value converted to type T, or the default value if retrieval or conversion fails.</returns>
    public T GetOptional<T>(string key, T defaultValue)
    {
        var value = _configuration[key];

        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        try
        {
            return ConvertValue<T>(value);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Attempts to get a configuration value and convert it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the configuration value to.</typeparam>
    /// <param name="key">The configuration key to retrieve.</param>
    /// <returns>
    /// A Result&lt;T&gt; containing the converted value on success, or a failure result
    /// if the key is not found or conversion fails.
    /// </returns>
    public Result<T> TryGet<T>(string key)
    {
        var value = _configuration[key];

        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail<T>(new Error("ConfigurationKeyNotFound", $"Configuration key '{key}' not found or empty."));

        try
        {
            var converted = ConvertValue<T>(value);
            return Result.Ok(converted);
        }
        catch (Exception ex)
        {
            var message = $"Failed to convert configuration key '{key}' to type '{typeof(T).Name}': {ex.Message}";
            return Result.Fail<T>(new Error("ConversionFailed", message));
        }
    }

    /// <summary>
    /// Checks whether a configuration key exists and is not empty.
    /// </summary>
    /// <param name="key">The configuration key to check.</param>
    /// <returns>True if the key exists and is not null or whitespace; otherwise false.</returns>
    public bool Exists(string key)
    {
        var value = _configuration[key];
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Converts a string value to the specified type using invariant culture for numeric types.
    /// </summary>
    /// <typeparam name="T">The target type to convert to.</typeparam>
    /// <param name="value">The string value to convert.</param>
    /// <returns>The converted value of type T.</returns>
    /// <exception cref="ConfigurationValidationException">
    /// Thrown when the conversion is not supported or the value cannot be converted.
    /// </exception>
    private static T ConvertValue<T>(string value)
    {
        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlyingType == typeof(string))
                return (T)(object)value;

            if (underlyingType == typeof(int))
                return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(long))
                return (T)(object)long.Parse(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(double))
                return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(decimal))
                return (T)(object)decimal.Parse(value, CultureInfo.InvariantCulture);

            if (underlyingType == typeof(bool))
                return (T)(object)bool.Parse(value);

            if (underlyingType == typeof(DateTime))
                return (T)(object)DateTime.Parse(value, CultureInfo.InvariantCulture);

            if (underlyingType.IsEnum)
                return (T)Enum.Parse(underlyingType, value, ignoreCase: true);

            throw new NotSupportedException($"Type conversion not supported for type '{targetType.Name}'.");
        }
        catch (NotSupportedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FormatException($"Unable to convert value '{value}' to type '{targetType.Name}'.", ex);
        }
    }
}
