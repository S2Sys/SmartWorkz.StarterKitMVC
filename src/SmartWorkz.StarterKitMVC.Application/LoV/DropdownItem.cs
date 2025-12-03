namespace SmartWorkz.StarterKitMVC.Application.LoV;

/// <summary>
/// Represents an item for dropdown/select controls.
/// </summary>
/// <param name="Value">The value to submit (e.g., "US").</param>
/// <param name="Text">The display text (e.g., "United States").</param>
/// <param name="Tags">Optional tags for filtering.</param>
/// <example>
/// <code>
/// var item = new DropdownItem("US", "United States", new[] { "north-america" });
/// 
/// // Use in HTML select
/// &lt;option value="@item.Value"&gt;@item.Text&lt;/option&gt;
/// </code>
/// </example>
public sealed record DropdownItem(string Value, string Text, IReadOnlyCollection<string>? Tags = null);
