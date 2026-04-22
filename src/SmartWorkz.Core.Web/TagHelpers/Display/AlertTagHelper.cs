using Microsoft.AspNetCore.Razor.TagHelpers;
using SmartWorkz.Web;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering Bootstrap alert components with optional dismiss functionality.
/// Targets the &lt;alert&gt; element and generates &lt;div class="alert alert-{type}"&gt;.
/// </summary>
/// <remarks>
/// Generates: &lt;div class="alert alert-{type} d-flex align-items-center [alert-dismissible fade show]"&gt;...&lt;/div&gt;
///
/// Bootstrap CSS Classes Applied:
/// - .alert: Base alert styling with padding and borders
/// - .alert-success: Green background for success messages (Type="success")
/// - .alert-danger: Red background for error/danger messages (Type="danger")
/// - .alert-warning: Yellow/orange background for warning messages (Type="warning")
/// - .alert-info: Blue background for informational messages (Type="info", default)
/// - .alert-dismissible: Applied when Dismissible=true, enables close button styling
/// - .fade and .show: Opacity and visibility for dismissible alerts
/// - .d-flex, .align-items-center: Flexbox layout for icon and message alignment
///
/// Icons: An appropriate icon is automatically included based on alert type:
/// - Success alerts show a checkmark icon
/// - Danger alerts show an error icon
/// - Warning alerts show a warning icon
/// - Info alerts show an info icon
///
/// Dismissible Alerts: When Dismissible=true (default), a close button (.btn-close) is rendered
/// with data-bs-dismiss="alert" attribute, allowing users to dismiss the alert dynamically.
///
/// Default Type: If Type is not specified or is an unknown value, "info" type is used.
/// </remarks>
/// <example>
/// &lt;!-- Simple success alert --&gt;
/// &lt;alert type="success" message="Profile updated successfully!" /&gt;
///
/// &lt;!-- Non-dismissible danger alert --&gt;
/// &lt;alert type="danger" message="An error occurred while saving." dismissible="false" /&gt;
///
/// &lt;!-- Warning alert with dismiss button --&gt;
/// &lt;alert type="warning" message="This action cannot be undone." /&gt;
///
/// &lt;!-- Default info alert --&gt;
/// &lt;alert message="Remember to save your changes regularly." /&gt;
/// </example>
[HtmlTargetElement("alert", Attributes = nameof(Type))]
public class AlertTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the alert type, which determines the Bootstrap alert class and icon.
    /// Supported values: "success", "danger", "warning", "info" (default).
    /// Maps to Bootstrap alert classes: .alert-success, .alert-danger, .alert-warning, .alert-info
    /// </summary>
    [HtmlAttributeName(nameof(Type))]
    public string Type { get; set; } = "info";

    /// <summary>
    /// Gets or sets the message text displayed in the alert.
    /// If empty or null, only the icon is shown.
    /// </summary>
    [HtmlAttributeName(nameof(Message))]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets whether the alert can be dismissed by the user.
    /// When true, adds .alert-dismissible class and renders a close button.
    /// Default is true.
    /// </summary>
    [HtmlAttributeName(nameof(Dismissible))]
    public bool Dismissible { get; set; } = true;

    private readonly IIconProvider _iconProvider;

    public AlertTagHelper(IIconProvider iconProvider)
    {
        _iconProvider = iconProvider ?? throw new ArgumentNullException(nameof(iconProvider));
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var alertClass = Type switch
        {
            "success" => "alert-success",
            "danger" => "alert-danger",
            "warning" => "alert-warning",
            _ => "alert-info"
        };

        var iconType = Type switch
        {
            "success" => IconType.Success,
            "danger" => IconType.Error,
            "warning" => IconType.Warning,
            _ => IconType.Info
        };

        var classAttr = $"alert {alertClass} d-flex align-items-center";
        if (Dismissible)
            classAttr += " alert-dismissible fade show";

        var closeBtn = Dismissible
            ? "<button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\" aria-label=\"Close\"></button>"
            : "";

        var icon = _iconProvider.GetIconHtml(iconType, "me-2 flex-shrink-0");
        var messageContent = string.IsNullOrEmpty(Message) ? "" : $"<div>{Message}</div>";

        var html = $"<div class=\"{classAttr}\">{icon}{messageContent}{closeBtn}</div>";

        output.TagName = null;
        output.Content.SetHtmlContent(html);
    }
}
