namespace SmartWorkz.StarterKitMVC.Application.EmailTemplates;

/// <summary>
/// Represents the result of rendering an email template.
/// </summary>
/// <param name="Subject">The rendered email subject.</param>
/// <param name="HtmlBody">The rendered HTML body (including header and footer).</param>
/// <param name="PlainTextBody">The rendered plain text body (optional).</param>
/// <param name="Success">Whether the rendering was successful.</param>
/// <param name="Errors">List of errors if rendering failed.</param>
public sealed record EmailTemplateRenderResult(
    string Subject,
    string HtmlBody,
    string? PlainTextBody,
    bool Success,
    IReadOnlyList<string> Errors
)
{
    /// <summary>
    /// Creates a successful render result.
    /// </summary>
    public static EmailTemplateRenderResult Ok(string subject, string htmlBody, string? plainTextBody = null)
        => new(subject, htmlBody, plainTextBody, true, Array.Empty<string>());
    
    /// <summary>
    /// Creates a failed render result.
    /// </summary>
    public static EmailTemplateRenderResult Fail(params string[] errors)
        => new(string.Empty, string.Empty, null, false, errors);
}
