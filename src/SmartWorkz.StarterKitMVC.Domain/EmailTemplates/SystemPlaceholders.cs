namespace SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

/// <summary>
/// Provides built-in system placeholders available in all email templates.
/// These placeholders are automatically populated by the template engine.
/// </summary>
public static class SystemPlaceholders
{
    /// <summary>Application name.</summary>
    public static readonly TemplatePlaceholder AppName = new()
    {
        Key = "{{AppName}}",
        DisplayName = "Application Name",
        Description = "The name of the application",
        Type = PlaceholderType.Text,
        IsRequired = false,
        SampleValue = "SmartWorkz"
    };
    
    /// <summary>Application URL.</summary>
    public static readonly TemplatePlaceholder AppUrl = new()
    {
        Key = "{{AppUrl}}",
        DisplayName = "Application URL",
        Description = "The base URL of the application",
        Type = PlaceholderType.Url,
        IsRequired = false,
        SampleValue = "https://example.com"
    };
    
    /// <summary>Recipient's name.</summary>
    public static readonly TemplatePlaceholder UserName = new()
    {
        Key = "{{UserName}}",
        DisplayName = "User Name",
        Description = "The recipient's full name",
        Type = PlaceholderType.Text,
        IsRequired = false,
        SampleValue = "John Doe"
    };
    
    /// <summary>Recipient's email.</summary>
    public static readonly TemplatePlaceholder UserEmail = new()
    {
        Key = "{{UserEmail}}",
        DisplayName = "User Email",
        Description = "The recipient's email address",
        Type = PlaceholderType.Text,
        IsRequired = false,
        SampleValue = "john.doe@example.com"
    };
    
    /// <summary>Current date.</summary>
    public static readonly TemplatePlaceholder CurrentDate = new()
    {
        Key = "{{CurrentDate}}",
        DisplayName = "Current Date",
        Description = "The current date when the email is sent",
        Type = PlaceholderType.Date,
        IsRequired = false,
        SampleValue = DateTime.Now.ToString("MMMM dd, yyyy")
    };
    
    /// <summary>Current year.</summary>
    public static readonly TemplatePlaceholder CurrentYear = new()
    {
        Key = "{{CurrentYear}}",
        DisplayName = "Current Year",
        Description = "The current year",
        Type = PlaceholderType.Text,
        IsRequired = false,
        SampleValue = DateTime.Now.Year.ToString()
    };
    
    /// <summary>Company name.</summary>
    public static readonly TemplatePlaceholder CompanyName = new()
    {
        Key = "{{CompanyName}}",
        DisplayName = "Company Name",
        Description = "The company or organization name",
        Type = PlaceholderType.Text,
        IsRequired = false,
        SampleValue = "SmartWorkz Inc."
    };
    
    /// <summary>Support email.</summary>
    public static readonly TemplatePlaceholder SupportEmail = new()
    {
        Key = "{{SupportEmail}}",
        DisplayName = "Support Email",
        Description = "The support email address",
        Type = PlaceholderType.Text,
        IsRequired = false,
        SampleValue = "support@example.com"
    };
    
    /// <summary>Unsubscribe URL.</summary>
    public static readonly TemplatePlaceholder UnsubscribeUrl = new()
    {
        Key = "{{UnsubscribeUrl}}",
        DisplayName = "Unsubscribe URL",
        Description = "Link for recipients to unsubscribe",
        Type = PlaceholderType.Url,
        IsRequired = false,
        SampleValue = "https://example.com/unsubscribe"
    };
    
    /// <summary>Company logo URL.</summary>
    public static readonly TemplatePlaceholder LogoUrl = new()
    {
        Key = "{{LogoUrl}}",
        DisplayName = "Logo URL",
        Description = "URL to the company logo image",
        Type = PlaceholderType.Image,
        IsRequired = false,
        SampleValue = "https://example.com/logo.png"
    };
    
    /// <summary>Privacy policy URL.</summary>
    public static readonly TemplatePlaceholder PrivacyUrl = new()
    {
        Key = "{{PrivacyUrl}}",
        DisplayName = "Privacy Policy URL",
        Description = "Link to the privacy policy",
        Type = PlaceholderType.Url,
        IsRequired = false,
        SampleValue = "https://example.com/privacy"
    };
    
    /// <summary>Terms of service URL.</summary>
    public static readonly TemplatePlaceholder TermsUrl = new()
    {
        Key = "{{TermsUrl}}",
        DisplayName = "Terms of Service URL",
        Description = "Link to the terms of service",
        Type = PlaceholderType.Url,
        IsRequired = false,
        SampleValue = "https://example.com/terms"
    };
    
    /// <summary>
    /// Gets all system placeholders.
    /// </summary>
    public static IReadOnlyList<TemplatePlaceholder> All => new[]
    {
        AppName,
        AppUrl,
        UserName,
        UserEmail,
        CurrentDate,
        CurrentYear,
        CompanyName,
        SupportEmail,
        UnsubscribeUrl,
        LogoUrl,
        PrivacyUrl,
        TermsUrl
    };
}
