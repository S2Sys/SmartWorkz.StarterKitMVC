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
