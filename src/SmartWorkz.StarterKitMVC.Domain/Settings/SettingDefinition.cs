namespace SmartWorkz.StarterKitMVC.Domain.Settings;

/// <summary>
/// Defines the data type of a setting value.
/// </summary>
public enum SettingValueType
{
    /// <summary>Plain text string.</summary>
    String,
    /// <summary>Integer number.</summary>
    Int,
    /// <summary>Boolean (true/false).</summary>
    Bool,
    /// <summary>Floating-point number.</summary>
    Double,
    /// <summary>Date and time.</summary>
    DateTime,
    /// <summary>Comma-separated list of strings.</summary>
    StringList,
    /// <summary>JSON object or array.</summary>
    Json,
    /// <summary>Encrypted string (for secrets).</summary>
    EncryptedString
}

/// <summary>
/// Defines a setting's metadata including key, type, and default value.
/// </summary>
/// <example>
/// <code>
/// var definition = new SettingDefinition
/// {
///     Id = Guid.NewGuid(),
///     Key = "app.theme",
///     CategoryKey = "appearance",
///     ValueType = SettingValueType.String,
///     DefaultValue = "light",
///     IsRequired = false
/// };
/// </code>
/// </example>
public sealed class SettingDefinition
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; init; }
    
    /// <summary>Unique setting key (e.g., "app.theme").</summary>
    public string Key { get; init; } = string.Empty;
    
    /// <summary>Category key for grouping.</summary>
    public string CategoryKey { get; init; } = string.Empty;
    
    /// <summary>Data type of the setting value.</summary>
    public SettingValueType ValueType { get; init; }
    
    /// <summary>Default value if not explicitly set.</summary>
    public string? DefaultValue { get; init; }
    
    /// <summary>Whether the setting is required.</summary>
    public bool IsRequired { get; init; }
}
