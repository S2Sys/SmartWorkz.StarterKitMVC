namespace SmartWorkz.Web.Models;

/// <summary>
/// Represents the complete definition of a dynamic form
/// </summary>
public class FormDefinition
{
    /// <summary>Form identifier</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Form title displayed to users</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Form description shown above fields</summary>
    public string? Description { get; set; }

    /// <summary>Collection of form fields</summary>
    public List<FormField> Fields { get; set; } = [];

    /// <summary>Submit button configuration</summary>
    public FormSubmitConfig SubmitConfig { get; set; } = new();

    /// <summary>Whether the form is currently disabled</summary>
    public bool IsDisabled { get; set; } = false;

    /// <summary>CSS class to apply to the form container</summary>
    public string? CssClass { get; set; }
}

/// <summary>
/// Represents a single field in a form
/// </summary>
public class FormField
{
    /// <summary>Unique field identifier</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Display label for the field</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Field type (text, email, password, number, select, checkbox, textarea, date)</summary>
    public string Type { get; set; } = "text";

    /// <summary>Default or current value</summary>
    public object? Value { get; set; }

    /// <summary>Placeholder text</summary>
    public string? Placeholder { get; set; }

    /// <summary>Help text displayed below field</summary>
    public string? HelpText { get; set; }

    /// <summary>Whether field is required</summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>Whether field is disabled</summary>
    public bool IsDisabled { get; set; } = false;

    /// <summary>Validation rules applied to field</summary>
    public List<FormValidationRule> ValidationRules { get; set; } = [];

    /// <summary>Options for select/dropdown fields</summary>
    public List<FormFieldOption> Options { get; set; } = [];

    /// <summary>Field name this field depends on for conditional visibility</summary>
    public string? DependsOn { get; set; }

    /// <summary>Value the dependent field must have to show this field</summary>
    public object? DependsOnValue { get; set; }

    /// <summary>CSS class for field styling</summary>
    public string? CssClass { get; set; }

    /// <summary>Field order in form (for sorting)</summary>
    public int Order { get; set; } = 0;

    /// <summary>Whether field is currently visible (computed based on conditional logic)</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsVisible { get; set; } = true;
}

/// <summary>
/// Represents an option in a select or dropdown field
/// </summary>
public class FormFieldOption
{
    /// <summary>Display text shown to user</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Value submitted with form</summary>
    public object Value { get; set; } = string.Empty;

    /// <summary>Whether option is disabled</summary>
    public bool IsDisabled { get; set; } = false;
}

/// <summary>
/// Represents a validation rule for a form field
/// </summary>
public class FormValidationRule
{
    /// <summary>Type of validation (required, email, minLength, maxLength, pattern, custom, min, max)</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Value for rule (e.g., minimum length, pattern regex)</summary>
    public string? Value { get; set; }

    /// <summary>Error message displayed when validation fails</summary>
    public string Message { get; set; } = "This field is invalid";

    /// <summary>Custom validation function name (for type="custom")</summary>
    public string? CustomFunction { get; set; }
}

/// <summary>
/// Configuration for form submission
/// </summary>
public class FormSubmitConfig
{
    /// <summary>Text on submit button</summary>
    public string SubmitButtonText { get; set; } = "Submit";

    /// <summary>Text on cancel button</summary>
    public string CancelButtonText { get; set; } = "Cancel";

    /// <summary>Whether to show cancel button</summary>
    public bool ShowCancelButton { get; set; } = true;

    /// <summary>Whether to show reset button</summary>
    public bool ShowResetButton { get; set; } = false;

    /// <summary>CSS class for submit button</summary>
    public string SubmitButtonClass { get; set; } = "btn btn-primary";

    /// <summary>CSS class for cancel button</summary>
    public string CancelButtonClass { get; set; } = "btn btn-secondary";

    /// <summary>CSS class for reset button</summary>
    public string ResetButtonClass { get; set; } = "btn btn-outline-secondary";
}

/// <summary>
/// Result returned after form submission
/// </summary>
public class FormSubmissionResult
{
    /// <summary>Whether submission was successful</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Message to display to user</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Submitted form data</summary>
    public Dictionary<string, object?> Data { get; set; } = [];

    /// <summary>Field-level validation errors</summary>
    public Dictionary<string, List<string>> FieldErrors { get; set; } = [];

    /// <summary>Additional metadata from submission</summary>
    public Dictionary<string, object?> Metadata { get; set; } = [];

    /// <summary>Optional redirect URL after successful submission</summary>
    public string? RedirectUrl { get; set; }
}

/// <summary>
/// Represents validation errors for a form
/// </summary>
public class FormValidationError
{
    /// <summary>Field name with error</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Error message</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Error code for programmatic handling</summary>
    public string? ErrorCode { get; set; }
}

/// <summary>
/// Represents an autocomplete item with a value and label for selection.
/// </summary>
/// <param name="Value">The unique value of the autocomplete item</param>
/// <param name="Label">The display label for the autocomplete item</param>
public record AutocompleteItem(string Value, string Label);

/// <summary>
/// Represents a file selected through the file upload component.
/// </summary>
/// <param name="Name">The name of the file</param>
/// <param name="Size">The size of the file in bytes</param>
/// <param name="Type">The MIME type of the file</param>
/// <param name="Data">The binary file data</param>
public record FileInfo(string Name, long Size, string Type, byte[] Data);

/// <summary>
/// Represents a breadcrumb item in a breadcrumb navigation trail.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// Unique identifier for the breadcrumb item, passed to the OnNavigate callback when clicked.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Display text shown to the user for this breadcrumb item.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Optional Bootstrap icon class (e.g., "bi bi-house") to display before the label.
    /// </summary>
    public string? Icon { get; set; }
}

/// <summary>
/// Represents a single tab in the TabsComponent with its header and content.
/// </summary>
public record TabItem
{
    /// <summary>
    /// Unique identifier for the tab, passed to the OnTabChanged callback when selected.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Display text shown in the tab header.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// RenderFragment containing the tab's content to display when active.
    /// </summary>
    public Microsoft.AspNetCore.Components.RenderFragment? Content { get; set; }
}

/// <summary>
/// Represents a single collapsible section in the AccordionComponent.
/// Each section has a title (header) and content that can be expanded or collapsed.
/// </summary>
public record AccordionItem
{
    /// <summary>
    /// Unique identifier for the accordion section, passed to the OnSectionChanged callback when toggled.
    /// Used to track which section is currently active.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Display text shown in the accordion section header (always visible).
    /// This is the clickable title that users interact with to expand/collapse the section.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// RenderFragment containing the accordion section's content to display when expanded.
    /// This content is hidden by default and shown when the section is active.
    /// </summary>
    public Microsoft.AspNetCore.Components.RenderFragment? Content { get; set; }
}

/// <summary>
/// Represents a single slide in the CarouselComponent.
/// Each slide can display an image with optional title and description, or custom RenderFragment content.
/// </summary>
public class CarouselItem
{
    /// <summary>
    /// Gets or sets the title displayed on the slide.
    /// Typically appears as an overlay on the image or above the content.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description text displayed on the slide.
    /// Typically appears below the title as supplementary text.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the image to display for this slide.
    /// If provided, the image is displayed as the slide's background or main content.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a custom RenderFragment to display as the slide content.
    /// If provided, this takes precedence over ImageUrl for content rendering.
    /// Allows for complex, interactive content within carousel slides.
    /// </summary>
    public Microsoft.AspNetCore.Components.RenderFragment? Content { get; set; }
}

/// <summary>
/// Represents a linear progress bar with support for multiple color variants, animation, and striped patterns.
/// Used to display the completion status of an operation as a percentage from 0 to 100.
/// </summary>
public class ProgressBar
{
    /// <summary>
    /// Gets or sets the progress percentage value (0-100).
    /// Values outside this range are automatically clamped to the valid range.
    /// </summary>
    public int Value { get; set; } = 0;

    /// <summary>
    /// Gets or sets the color variant of the progress bar.
    /// Valid values: "primary" (default), "success", "warning", "danger".
    /// Invalid variants fall back to "primary".
    /// </summary>
    public string Variant { get; set; } = "primary";

    /// <summary>
    /// Gets or sets whether to display the percentage label on the progress bar.
    /// When true, displays the current percentage value (e.g., "75%").
    /// Default is true.
    /// </summary>
    public bool ShowLabel { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to animate the progress bar.
    /// When true, applies a moving stripes animation effect to the bar.
    /// Default is false.
    /// </summary>
    public bool Animated { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to display a striped pattern on the progress bar.
    /// When true, applies a diagonal striped pattern to the bar.
    /// Default is false.
    /// </summary>
    public bool Striped { get; set; } = false;

    /// <summary>
    /// Gets or sets the height of the progress bar using CSS units.
    /// Default is "1rem". Examples: "0.5rem", "2rem", "25px".
    /// </summary>
    public string Height { get; set; } = "1rem";
}
