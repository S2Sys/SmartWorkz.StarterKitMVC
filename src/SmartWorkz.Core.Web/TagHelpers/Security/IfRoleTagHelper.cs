using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering content only if the user belongs to a specific role.
/// Targets the &lt;if-role&gt; element and conditionally renders content based on role membership.
/// </summary>
/// <remarks>
/// Checks if the current user is authenticated and belongs to at least one of the specified roles.
/// Supports comma-separated role names (CSV) with OR logic: content is shown if user has ANY of the specified roles.
///
/// Authentication Check: First verifies User.Identity.IsAuthenticated. If false, content is suppressed.
/// Role Lookup: Uses User.IsInRole(role) to check each specified role. Checks are case-insensitive.
/// Multiple Roles: Comma-separated roles are trimmed and checked individually (OR logic).
/// The first matching role causes content to be rendered; if no match, content is suppressed.
///
/// HTML Output: When authorized, the &lt;if-role&gt; tag is replaced with its inner content (tag name set to null).
/// When not authorized (not authenticated or role mismatch), all output is suppressed.
///
/// Role Comparison: ASP.NET Core's IsInRole() method typically performs case-insensitive comparison,
/// but exact behavior depends on the configured authentication and authorization handler.
/// Best practice: Use consistent role naming (e.g., "Admin", "Manager", "User").
///
/// Typical Usage: Display role-specific UI like admin buttons, manager dashboards, or user-only features.
/// Use in navigation menus, action buttons, or feature sections restricted by role.
/// Common roles: "Admin", "Manager", "User", "Guest", "Moderator", custom application roles.
///
/// ⚠️ **Security Note:** Hidden UI elements are still rendered in the HTML and visible in page source.
/// For true security, always validate roles on the server side using [Authorize(Roles = "...")] attributes
/// and policy-based authorization. Never rely solely on client-side role checks for security-critical functionality.
/// Sensitive operations must be protected by server-side authorization checks.
///
/// ♿ **Accessibility Note:** Content is hidden from view but remains in the DOM.
/// Screen readers may still access hidden content. Consider using aria-hidden="true" on an outer wrapper
/// for content that should be completely hidden from assistive technologies.
///
/// Example: &lt;if-role role="Admin,Manager"&gt;...&lt;/if-role&gt; (shows if user is Admin or Manager)
/// </remarks>
/// <param name="Role">
/// Comma-separated list of allowed role names (e.g., "Admin,Manager" or "Editor,Contributor").
/// Uses OR logic: content is shown if the user has ANY of the specified roles.
/// Whitespace around role names is trimmed automatically.
/// Role comparison is case-insensitive (depends on authorization configuration).
/// Required attribute.
/// </param>
/// <example>
/// &lt;!-- Simple role check: show delete button if user is Admin --&gt;
/// &lt;if-role role="Admin"&gt;
///   &lt;button class="btn btn-danger" onclick="deleteItem()"&gt;Delete&lt;/button&gt;
/// &lt;/if-role&gt;
///
/// &lt;!-- Multiple roles (OR logic): show if user is Admin or Manager --&gt;
/// &lt;if-role role="Admin,Manager"&gt;
///   &lt;a href="/admin" class="btn btn-primary"&gt;Admin Panel&lt;/a&gt;
/// &lt;/if-role&gt;
///
/// &lt;!-- Editor dashboard with edit/delete options --&gt;
/// &lt;if-role role="Editor"&gt;
///   &lt;div class="editor-toolbar"&gt;
///     &lt;button class="btn btn-info" onclick="editItem()"&gt;Edit&lt;/button&gt;
///     &lt;button class="btn btn-warning" onclick="publishItem()"&gt;Publish&lt;/button&gt;
///   &lt;/div&gt;
/// &lt;/if-role&gt;
///
/// &lt;!-- Multiple role sections with different features --&gt;
/// &lt;div class="dashboard"&gt;
///   &lt;if-role role="Admin"&gt;
///     &lt;div class="admin-section"&gt;
///       &lt;h3&gt;System Administration&lt;/h3&gt;
///       &lt;a href="/admin/users"&gt;Manage Users&lt;/a&gt;
///       &lt;a href="/admin/settings"&gt;System Settings&lt;/a&gt;
///     &lt;/div&gt;
///   &lt;/if-role&gt;
///   &lt;if-role role="Manager,Editor"&gt;
///     &lt;div class="content-section"&gt;
///       &lt;h3&gt;Content Management&lt;/h3&gt;
///       &lt;a href="/content"&gt;View Content&lt;/a&gt;
///     &lt;/div&gt;
///   &lt;/if-role&gt;
/// &lt;/div&gt;
///
/// &lt;!-- Nested role checks in a complex menu --&gt;
/// &lt;nav class="sidebar"&gt;
///   &lt;ul&gt;
///     &lt;li&gt;&lt;a href="/home"&gt;Home&lt;/a&gt;&lt;/li&gt;
///     &lt;if-role role="User,Admin,Manager"&gt;
///       &lt;li&gt;&lt;a href="/dashboard"&gt;Dashboard&lt;/a&gt;&lt;/li&gt;
///       &lt;li&gt;&lt;a href="/profile"&gt;My Profile&lt;/a&gt;&lt;/li&gt;
///     &lt;/if-role&gt;
///     &lt;if-role role="Admin"&gt;
///       &lt;li&gt;&lt;a href="/admin"&gt;Administration&lt;/a&gt;&lt;/li&gt;
///     &lt;/if-role&gt;
///   &lt;/ul&gt;
/// &lt;/nav&gt;
///
/// &lt;!-- Contributor role with limited permissions --&gt;
/// &lt;if-role role="Contributor,Editor,Admin"&gt;
///   &lt;div class="publish-section"&gt;
///     &lt;button class="btn btn-success" onclick="submit()"&gt;Submit for Review&lt;/button&gt;
///   &lt;/div&gt;
/// &lt;/if-role&gt;
/// </example>
[HtmlTargetElement("if-role")]
public class IfRoleTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the role name or comma-separated list of allowed role names.
    /// Multiple roles are separated by commas and whitespace is trimmed from each role.
    /// Uses OR logic: content is shown if the user has ANY of the specified roles.
    /// Role comparison is case-insensitive (standard behavior for User.IsInRole()).
    /// Required attribute for this TagHelper to function.
    /// </summary>
    [HtmlAttributeName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets the ViewContext used to access the current HttpContext and User.
    /// Automatically injected by the Razor engine.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var user = ViewContext?.HttpContext?.User;
        if (user == null || !(user.Identity?.IsAuthenticated ?? false))
        {
            output.SuppressOutput();
            return;
        }

        if (string.IsNullOrWhiteSpace(Role))
        {
            output.SuppressOutput();
            return;
        }

        // Support comma-separated roles with OR logic
        var roles = Role
            .Split(',')
            .Select(r => r.Trim())
            .Where(r => !string.IsNullOrEmpty(r))
            .ToArray();

        var hasRole = roles.Any(role => user.IsInRole(role));

        if (hasRole)
        {
            output.TagName = null; // Remove the if-role tag wrapper
        }
        else
        {
            output.SuppressOutput();
        }
    }
}
