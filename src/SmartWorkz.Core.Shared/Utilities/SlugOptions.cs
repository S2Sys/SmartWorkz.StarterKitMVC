namespace SmartWorkz.Core.Shared.Utilities;

/// <summary>
/// Options for configuring slug generation behavior in <see cref="SlugHelper"/>.
/// </summary>
public class SlugOptions
{
    /// <summary>
    /// Whether to convert the slug to lowercase. Default is true.
    /// </summary>
    public bool Lowercase { get; set; } = true;

    /// <summary>
    /// Maximum length of the generated slug. 0 means no limit. Default is 100.
    /// </summary>
    public int MaxLength { get; set; } = 100;

    /// <summary>
    /// Character or string to use as a separator between words. Default is "-".
    /// </summary>
    public string Separator { get; set; } = "-";

    /// <summary>
    /// Whether to remove accented characters (é → e, ñ → n, etc.). Default is true.
    /// </summary>
    public bool RemoveAccents { get; set; } = true;

    /// <summary>
    /// Whether to remove special characters. Default is true.
    /// </summary>
    public bool RemoveSpecialChars { get; set; } = true;
}
