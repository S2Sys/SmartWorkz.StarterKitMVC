using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering content only if the user has a specific claim with a matching value.
/// Targets the &lt;if-claim&gt; element and conditionally renders content based on claim verification.
/// </summary>
/// <remarks>
/// Checks if the current user is authenticated and possesses the specified claim type with one of the specified values.
/// Supports comma-separated claim values (CSV) with OR logic: content is shown if user has ANY of the specified values.
///
/// Authentication Check: First verifies User.Identity.IsAuthenticated. If false, content is suppressed.
/// Claim Lookup: Uses User.FindFirst(ClaimType) to locate the specified claim.
/// Value Matching: If ClaimValue is null or empty, the claim existence alone satisfies the condition.
/// CSV Parsing: Comma-separated values in ClaimValue are trimmed and compared individually (OR logic).
/// The first matching value causes content to be rendered; if no match, content is suppressed.
///
/// HTML Output: When authorized, the &lt;if-claim&gt; tag is replaced with its inner content (tag name set to null).
/// When not authorized (not authenticated, claim missing, or value mismatch), all output is suppressed.
///
/// Typical Usage: Display content based on custom claims (department, permissions, subscription level).
/// Use in admin dashboards, role-based dashboards, or feature-restricted sections.
/// Common claim types: "role", "department", "permission", "subscription_level", custom claim URIs.
///
/// ⚠️ **Security Note:** Hidden UI elements are still rendered in the HTML and visible in page source.
/// For true security, always validate claims on the server side using policy-based authorization [Authorize(Policy = "...")].
/// Never rely solely on client-side claim checks for security-critical functionality.
/// Claims should only display UI hints; actual authorization must occur server-side.
///
/// ♿ **Accessibility Note:** Content is hidden from view but remains in the DOM.
/// Screen readers may still access hidden content. Consider using aria-hidden="true" on an outer wrapper
/// for content that should be completely hidden from assistive technologies.
///
/// Example: &lt;if-claim type="department" value="IT,Finance"&gt;...&lt;/if-claim&gt; (shows if user is in IT or Finance)
/// </remarks>
/// <param name="ClaimType">
/// The claim type to search for in the user's claims (e.g., "role", "department", "http://example.com/permission").
/// Uses ClaimTypes for standard claim types like ClaimTypes.Role, ClaimTypes.Email, etc.
/// Required attribute.
/// </param>
/// <param name="ClaimValue">
/// Comma-separated list of allowed claim values (e.g., "Admin,Manager" or "Premium,Enterprise").
/// Uses OR logic: content is shown if the claim matches ANY value in the list.
/// Whitespace around values is trimmed automatically.
/// If null or empty, the claim's existence alone is sufficient (any value matches).
/// </param>
/// <example>
/// &lt;!-- Simple claim check: show if user has "role" claim with value "Admin" --&gt;
/// &lt;if-claim type="role" value="Admin"&gt;
///   &lt;button class="btn btn-danger"&gt;Delete User&lt;/button&gt;
/// &lt;/if-claim&gt;
///
/// &lt;!-- Multiple values (OR logic): show if user is Admin or Manager --&gt;
/// &lt;if-claim type="role" value="Admin,Manager"&gt;
///   &lt;a href="/admin" class="btn btn-primary"&gt;Admin Panel&lt;/a&gt;
/// &lt;/if-claim&gt;
///
/// &lt;!-- Department-based access: show if in IT or Finance department --&gt;
/// &lt;if-claim type="department" value="IT,Finance"&gt;
///   &lt;div class="reports-section"&gt;
///     &lt;h3&gt;Financial Reports&lt;/h3&gt;
///     &lt;a href="/reports/finance"&gt;View Reports&lt;/a&gt;
///   &lt;/div&gt;
/// &lt;/if-claim&gt;
///
/// &lt;!-- Subscription level: show premium features if subscription is Premium or Enterprise --&gt;
/// &lt;if-claim type="subscription_level" value="Premium,Enterprise"&gt;
///   &lt;div class="premium-features"&gt;
///     &lt;h3&gt;Advanced Analytics&lt;/h3&gt;
///     &lt;p&gt;You have access to advanced reporting features.&lt;/p&gt;
///   &lt;/div&gt;
/// &lt;/if-claim&gt;
///
/// &lt;!-- Claim existence without value check: show if user has ANY custom-permission claim --&gt;
/// &lt;if-claim type="custom-permission"&gt;
///   &lt;p&gt;You have special permissions.&lt;/p&gt;
/// &lt;/if-claim&gt;
///
/// &lt;!-- Combined with other markup: admin toolbar with multiple restrictions --&gt;
/// &lt;div class="admin-toolbar"&gt;
///   &lt;if-claim type="role" value="Admin"&gt;
///     &lt;button class="btn btn-danger" onclick="deleteAll()"&gt;Delete All&lt;/button&gt;
///   &lt;/if-claim&gt;
///   &lt;if-claim type="role" value="Admin,Moderator"&gt;
///     &lt;button class="btn btn-warning" onclick="moderate()"&gt;Moderate&lt;/button&gt;
///   &lt;/if-claim&gt;
/// &lt;/div&gt;
/// </example>
[HtmlTargetElement("if-claim")]
public class IfClaimTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the claim type to check (e.g., "role", "department", "email", or custom claim URI).
    /// Maps to User.FindFirst(ClaimType).
    /// Required attribute for this TagHelper to function.
    /// </summary>
    [HtmlAttributeName("type")]
    public string? ClaimType { get; set; }

    /// <summary>
    /// Gets or sets the claim value or comma-separated list of allowed values.
    /// Multiple values are separated by commas and whitespace is trimmed from each value.
    /// Uses OR logic: content is shown if the claim matches ANY value in this list.
    /// If null or empty, the existence of the claim alone (with any value) satisfies the condition.
    /// Default is null (any claim value matches).
    /// </summary>
    [HtmlAttributeName("value")]
    public string? ClaimValue { get; set; }

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

        if (string.IsNullOrWhiteSpace(ClaimType))
        {
            output.SuppressOutput();
            return;
        }

        var claim = user.FindFirst(ClaimType);
        if (claim == null)
        {
            output.SuppressOutput();
            return;
        }

        // If no claim value specified, just the existence of the claim is sufficient
        if (string.IsNullOrWhiteSpace(ClaimValue))
        {
            output.TagName = null; // Remove the if-claim tag wrapper
            return;
        }

        // Support comma-separated values with OR logic
        var allowedValues = ClaimValue
            .Split(',')
            .Select(v => v.Trim())
            .Where(v => !string.IsNullOrEmpty(v))
            .ToArray();

        if (allowedValues.Contains(claim.Value, StringComparer.OrdinalIgnoreCase))
        {
            output.TagName = null; // Remove the if-claim tag wrapper
        }
        else
        {
            output.SuppressOutput();
        }
    }
}
