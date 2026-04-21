namespace SmartWorkz.Shared;

/// <summary>
/// Provides typed access to configuration values with automatic type conversion and validation.
/// </summary>
public interface IConfigurationHelper
{
    /// <summary>
    /// Gets a required configuration value and converts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the configuration value to.</typeparam>
    /// <param name="key">The configuration key to retrieve.</param>
    /// <returns>The configuration value converted to type T.</returns>
    /// <exception cref="ConfigurationValidationException">
    /// Thrown when the configuration key is not found, is empty, or cannot be converted to type T.
    /// </exception>
    T GetRequired<T>(string key);

    /// <summary>
    /// Gets an optional configuration value and converts it to the specified type,
    /// returning a default value if the key is not found or conversion fails.
    /// </summary>
    /// <typeparam name="T">The type to convert the configuration value to.</typeparam>
    /// <param name="key">The configuration key to retrieve.</param>
    /// <param name="defaultValue">The value to return if the key is not found or conversion fails.</param>
    /// <returns>The configuration value converted to type T, or the default value if retrieval or conversion fails.</returns>
    T GetOptional<T>(string key, T defaultValue);

    /// <summary>
    /// Attempts to get a configuration value and convert it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the configuration value to.</typeparam>
    /// <param name="key">The configuration key to retrieve.</param>
    /// <returns>
    /// A Result&lt;T&gt; containing the converted value on success, or a failure result
    /// if the key is not found or conversion fails.
    /// </returns>
    Result<T> TryGet<T>(string key);

    /// <summary>
    /// Checks whether a configuration key exists and is not empty.
    /// </summary>
    /// <param name="key">The configuration key to check.</param>
    /// <returns>True if the key exists and is not null or whitespace; otherwise false.</returns>
    bool Exists(string key);
}
