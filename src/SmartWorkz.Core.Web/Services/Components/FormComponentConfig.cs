namespace SmartWorkz.Core.Web.Services.Components;

/// <summary>
/// Configuration class for form component styling with customizable Bootstrap CSS classes.
/// Contains 18 properties for various form components and their variants.
/// </summary>
public class FormComponentConfig
{
    /// <summary>
    /// Gets or sets the CSS class for standard form inputs. Default: "form-control"
    /// </summary>
    public string InputClass { get; set; } = "form-control";

    /// <summary>
    /// Gets or sets the CSS class for small form inputs. Default: "form-control-sm"
    /// </summary>
    public string InputSmallClass { get; set; } = "form-control-sm";

    /// <summary>
    /// Gets or sets the CSS class for large form inputs. Default: "form-control-lg"
    /// </summary>
    public string InputLargeClass { get; set; } = "form-control-lg";

    /// <summary>
    /// Gets or sets the CSS class for form labels. Default: "form-label"
    /// </summary>
    public string LabelClass { get; set; } = "form-label";

    /// <summary>
    /// Gets or sets the CSS class for standard buttons. Default: "btn"
    /// </summary>
    public string ButtonClass { get; set; } = "btn";

    /// <summary>
    /// Gets or sets the CSS class for primary buttons. Default: "btn-primary"
    /// </summary>
    public string ButtonPrimaryClass { get; set; } = "btn-primary";

    /// <summary>
    /// Gets or sets the CSS class for secondary buttons. Default: "btn-secondary"
    /// </summary>
    public string ButtonSecondaryClass { get; set; } = "btn-secondary";

    /// <summary>
    /// Gets or sets the CSS class for danger buttons. Default: "btn-danger"
    /// </summary>
    public string ButtonDangerClass { get; set; } = "btn-danger";

    /// <summary>
    /// Gets or sets the CSS class for success buttons. Default: "btn-success"
    /// </summary>
    public string ButtonSuccessClass { get; set; } = "btn-success";

    /// <summary>
    /// Gets or sets the CSS class for warning buttons. Default: "btn-warning"
    /// </summary>
    public string ButtonWarningClass { get; set; } = "btn-warning";

    /// <summary>
    /// Gets or sets the CSS class for validation error state. Default: "is-invalid"
    /// </summary>
    public string ValidationErrorClass { get; set; } = "is-invalid";

    /// <summary>
    /// Gets or sets the CSS class for validation success state. Default: "is-valid"
    /// </summary>
    public string ValidationSuccessClass { get; set; } = "is-valid";

    /// <summary>
    /// Gets or sets the CSS class for form groups. Default: "mb-3"
    /// </summary>
    public string FormGroupClass { get; set; } = "mb-3";

    /// <summary>
    /// Gets or sets the CSS class for success alerts. Default: "alert-success"
    /// </summary>
    public string AlertSuccessClass { get; set; } = "alert-success";

    /// <summary>
    /// Gets or sets the CSS class for error/danger alerts. Default: "alert-danger"
    /// </summary>
    public string AlertErrorClass { get; set; } = "alert-danger";

    /// <summary>
    /// Gets or sets the CSS class for warning alerts. Default: "alert-warning"
    /// </summary>
    public string AlertWarningClass { get; set; } = "alert-warning";

    /// <summary>
    /// Gets or sets the CSS class for info alerts. Default: "alert-info"
    /// </summary>
    public string AlertInfoClass { get; set; } = "alert-info";
}
