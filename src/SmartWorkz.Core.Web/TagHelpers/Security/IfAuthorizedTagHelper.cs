using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SmartWorkz.Web;

/// <summary>
/// TagHelper for rendering content only if the user is authenticated.
/// Targets the &lt;if-authorized&gt; element and conditionally renders its content based on authentication status.
/// </summary>
/// <remarks>
/// Renders the inner content only if the current user is authenticated (User.Identity?.IsAuthenticated is true).
/// If the user is not authenticated, the content is suppressed and the custom tag is removed.
///
/// Authentication Check: Uses ViewContext to access HttpContext.User.Identity.IsAuthenticated.
/// If no user context is available, content is suppressed (treated as unauthorized).
///
/// HTML Output: When authorized, the &lt;if-authorized&gt; tag is replaced with its inner content (tag name set to null).
/// When not authorized, all output is suppressed using output.SuppressOutput().
///
/// Typical Usage: Display user-specific content like logout buttons, user profiles, or dashboard links
/// on authenticated pages. Use in header navigation, user menu, or admin dashboard areas.
///
/// ⚠️ **Security Note:** Hidden UI elements are still rendered in the HTML and visible in page source.
/// For true security, always validate authorization on the server side using [Authorize] attributes,
/// policy-based authorization, and server-side checks for sensitive operations.
/// Never rely solely on client-side UI hiding for security-critical functionality.
///
/// ♿ **Accessibility Note:** Content is hidden from view but remains in the DOM.
/// Screen readers may still access hidden content. Consider using aria-hidden="true" on an outer wrapper
/// for content that should be completely hidden from assistive technologies.
///
/// Example with Bootstrap: &lt;if-authorized&gt;&lt;nav class="navbar"&gt;...&lt;/nav&gt;&lt;/if-authorized&gt;
/// </remarks>
/// <example>
/// &lt;!-- Simple logout button, shown only if authenticated --&gt;
/// &lt;if-authorized&gt;
///   &lt;button class="btn btn-danger"&gt;Logout&lt;/button&gt;
/// &lt;/if-authorized&gt;
///
/// &lt;!-- User profile section in header --&gt;
/// &lt;if-authorized&gt;
///   &lt;div class="user-profile"&gt;
///     &lt;span&gt;Welcome, @Model.User.Name!&lt;/span&gt;
///     &lt;a href="/profile"&gt;View Profile&lt;/a&gt;
///   &lt;/div&gt;
/// &lt;/if-authorized&gt;
///
/// &lt;!-- Multiple buttons and links for authenticated users --&gt;
/// &lt;if-authorized&gt;
///   &lt;div class="authenticated-menu"&gt;
///     &lt;a href="/dashboard" class="btn btn-primary"&gt;Dashboard&lt;/a&gt;
///     &lt;a href="/orders" class="btn btn-info"&gt;My Orders&lt;/a&gt;
///     &lt;button onclick="logout()" class="btn btn-danger"&gt;Logout&lt;/button&gt;
///   &lt;/div&gt;
/// &lt;/if-authorized&gt;
///
/// &lt;!-- Unauthenticated content shown separately --&gt;
/// &lt;div&gt;
///   &lt;if-authorized&gt;
///     &lt;p&gt;You are logged in.&lt;/p&gt;
///   &lt;/if-authorized&gt;
///   &lt;!-- Note: For not-authenticated content, use negation in IfAuthorizedTagHelper
///        or implement a separate IfNotAuthorizedTagHelper --&gt;
/// &lt;/div&gt;
/// </example>
[HtmlTargetElement("if-authorized")]
public class IfAuthorizedTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the ViewContext used to access the current HttpContext and User.
    /// Automatically injected by the Razor engine.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var isAuthenticated = ViewContext?.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        if (!isAuthenticated)
        {
            output.SuppressOutput();
        }
        else
        {
            output.TagName = null; // Remove the if-authorized tag wrapper
        }
    }
}
