using SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

namespace SmartWorkz.StarterKitMVC.Infrastructure.EmailTemplates;

/// <summary>
/// Provides default email templates and sections for initial setup.
/// </summary>
public static class DefaultEmailTemplates
{
    /// <summary>
    /// Default header section with company branding.
    /// </summary>
    public static EmailTemplateSection DefaultHeader => new()
    {
        Id = "header-default",
        Name = "Default Header",
        Type = SectionType.Header,
        IsDefault = true,
        IsActive = true,
        HtmlContent = """
            <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px 20px; text-align: center;">
                <img src="{{LogoUrl}}" alt="{{AppName}}" style="max-height: 50px; margin-bottom: 10px;" onerror="this.style.display='none'">
                <h1 style="color: #ffffff; margin: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; font-size: 24px;">
                    {{AppName}}
                </h1>
            </div>
            """
    };

    /// <summary>
    /// Default footer section with links and copyright.
    /// </summary>
    public static EmailTemplateSection DefaultFooter => new()
    {
        Id = "footer-default",
        Name = "Default Footer",
        Type = SectionType.Footer,
        IsDefault = true,
        IsActive = true,
        HtmlContent = """
            <div style="background-color: #f8f9fa; padding: 30px 20px; text-align: center; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">
                <p style="margin: 0 0 10px 0; color: #6c757d; font-size: 14px;">
                    <a href="{{PrivacyUrl}}" style="color: #667eea; text-decoration: none;">Privacy Policy</a> |
                    <a href="{{TermsUrl}}" style="color: #667eea; text-decoration: none;">Terms of Service</a> |
                    <a href="{{UnsubscribeUrl}}" style="color: #667eea; text-decoration: none;">Unsubscribe</a>
                </p>
                <p style="margin: 0 0 10px 0; color: #6c757d; font-size: 14px;">
                    {{CompanyName}} | <a href="mailto:{{SupportEmail}}" style="color: #667eea;">{{SupportEmail}}</a>
                </p>
                <p style="margin: 0; color: #adb5bd; font-size: 12px;">
                    © {{CurrentYear}} {{CompanyName}}. All rights reserved.
                </p>
            </div>
            """
    };

    /// <summary>
    /// Minimal header for transactional emails.
    /// </summary>
    public static EmailTemplateSection MinimalHeader => new()
    {
        Id = "header-minimal",
        Name = "Minimal Header",
        Type = SectionType.Header,
        IsDefault = false,
        IsActive = true,
        HtmlContent = """
            <div style="padding: 20px; border-bottom: 3px solid #667eea;">
                <h2 style="color: #333; margin: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">
                    {{AppName}}
                </h2>
            </div>
            """
    };

    /// <summary>
    /// Minimal footer for transactional emails.
    /// </summary>
    public static EmailTemplateSection MinimalFooter => new()
    {
        Id = "footer-minimal",
        Name = "Minimal Footer",
        Type = SectionType.Footer,
        IsDefault = false,
        IsActive = true,
        HtmlContent = """
            <div style="padding: 20px; border-top: 1px solid #dee2e6; text-align: center;">
                <p style="margin: 0; color: #6c757d; font-size: 12px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;">
                    © {{CurrentYear}} {{CompanyName}}
                </p>
            </div>
            """
    };

    /// <summary>
    /// Welcome email template for new user registration.
    /// </summary>
    public static EmailTemplate WelcomeEmail => new()
    {
        Id = "welcome-email",
        Name = "Welcome Email",
        Description = "Sent to new users after registration",
        Category = "Onboarding",
        Subject = "Welcome to {{AppName}}, {{UserName}}!",
        HeaderId = "header-default",
        FooterId = "footer-default",
        IsActive = true,
        IsSystem = true,
        Tags = new List<string> { "onboarding", "welcome", "registration" },
        Placeholders = new List<TemplatePlaceholder>
        {
            new() { Key = "{{UserName}}", DisplayName = "User Name", IsRequired = true, Type = PlaceholderType.Text, SampleValue = "John Doe" },
            new() { Key = "{{LoginUrl}}", DisplayName = "Login URL", IsRequired = false, Type = PlaceholderType.Url, SampleValue = "https://example.com/login" }
        },
        BodyContent = """
            <div style="padding: 40px 20px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #ffffff;">
                <h2 style="color: #333; margin: 0 0 20px 0;">Welcome aboard, {{UserName}}! 🎉</h2>
                <p style="color: #555; font-size: 16px; line-height: 1.6;">
                    Thank you for joining {{AppName}}. We're excited to have you as part of our community!
                </p>
                <p style="color: #555; font-size: 16px; line-height: 1.6;">
                    Your account has been created successfully. You can now access all the features and start exploring.
                </p>
                <div style="text-align: center; margin: 30px 0;">
                    <a href="{{LoginUrl}}" style="display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;">
                        Get Started
                    </a>
                </div>
                <p style="color: #555; font-size: 16px; line-height: 1.6;">
                    If you have any questions, feel free to reach out to our support team at 
                    <a href="mailto:{{SupportEmail}}" style="color: #667eea;">{{SupportEmail}}</a>.
                </p>
                <p style="color: #555; font-size: 16px; line-height: 1.6; margin-top: 30px;">
                    Best regards,<br>
                    <strong>The {{AppName}} Team</strong>
                </p>
            </div>
            """,
        PlainTextContent = """
            Welcome aboard, {{UserName}}!

            Thank you for joining {{AppName}}. We're excited to have you as part of our community!

            Your account has been created successfully. You can now access all the features and start exploring.

            Get Started: {{LoginUrl}}

            If you have any questions, feel free to reach out to our support team at {{SupportEmail}}.

            Best regards,
            The {{AppName}} Team
            """
    };

    /// <summary>
    /// Password reset email template.
    /// </summary>
    public static EmailTemplate PasswordResetEmail => new()
    {
        Id = "password-reset",
        Name = "Password Reset",
        Description = "Sent when a user requests a password reset",
        Category = "Security",
        Subject = "Reset Your {{AppName}} Password",
        HeaderId = "header-default",
        FooterId = "footer-default",
        IsActive = true,
        IsSystem = true,
        Tags = new List<string> { "security", "password", "reset" },
        Placeholders = new List<TemplatePlaceholder>
        {
            new() { Key = "{{UserName}}", DisplayName = "User Name", IsRequired = true, Type = PlaceholderType.Text, SampleValue = "John Doe" },
            new() { Key = "{{ResetUrl}}", DisplayName = "Reset URL", IsRequired = true, Type = PlaceholderType.Url, SampleValue = "https://example.com/reset?token=abc123" },
            new() { Key = "{{ExpiryHours}}", DisplayName = "Link Expiry Hours", IsRequired = false, Type = PlaceholderType.Number, DefaultValue = "24", SampleValue = "24" }
        },
        BodyContent = """
            <div style="padding: 40px 20px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #ffffff;">
                <h2 style="color: #333; margin: 0 0 20px 0;">Password Reset Request</h2>
                <p style="color: #555; font-size: 16px; line-height: 1.6;">
                    Hi {{UserName}},
                </p>
                <p style="color: #555; font-size: 16px; line-height: 1.6;">
                    We received a request to reset your password for your {{AppName}} account. Click the button below to create a new password:
                </p>
                <div style="text-align: center; margin: 30px 0;">
                    <a href="{{ResetUrl}}" style="display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;">
                        Reset Password
                    </a>
                </div>
                <p style="color: #888; font-size: 14px; line-height: 1.6;">
                    This link will expire in {{ExpiryHours}} hours. If you didn't request a password reset, you can safely ignore this email.
                </p>
                <p style="color: #555; font-size: 16px; line-height: 1.6; margin-top: 30px;">
                    Best regards,<br>
                    <strong>The {{AppName}} Team</strong>
                </p>
            </div>
            """
    };

    /// <summary>
    /// Email verification template.
    /// </summary>
    public static EmailTemplate EmailVerification => new()
    {
        Id = "email-verification",
        Name = "Email Verification",
        Description = "Sent to verify user email address",
        Category = "Security",
        Subject = "Verify Your Email Address - {{AppName}}",
        HeaderId = "header-default",
        FooterId = "footer-default",
        IsActive = true,
        IsSystem = true,
        Tags = new List<string> { "security", "verification", "email" },
        Placeholders = new List<TemplatePlaceholder>
        {
            new() { Key = "{{UserName}}", DisplayName = "User Name", IsRequired = true, Type = PlaceholderType.Text, SampleValue = "John Doe" },
            new() { Key = "{{VerificationUrl}}", DisplayName = "Verification URL", IsRequired = true, Type = PlaceholderType.Url, SampleValue = "https://example.com/verify?token=abc123" },
            new() { Key = "{{VerificationCode}}", DisplayName = "Verification Code", IsRequired = false, Type = PlaceholderType.Text, SampleValue = "123456" }
        },
        BodyContent = """
            <div style="padding: 40px 20px; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #ffffff;">
                <h2 style="color: #333; margin: 0 0 20px 0;">Verify Your Email Address</h2>
                <p style="color: #555; font-size: 16px; line-height: 1.6;">
                    Hi {{UserName}},
                </p>
                <p style="color: #555; font-size: 16px; line-height: 1.6;">
                    Please verify your email address to complete your {{AppName}} account setup.
                </p>
                <div style="text-align: center; margin: 30px 0;">
                    <a href="{{VerificationUrl}}" style="display: inline-block; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: #ffffff; padding: 14px 40px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 16px;">
                        Verify Email
                    </a>
                </div>
                <p style="color: #555; font-size: 16px; line-height: 1.6;">
                    Or use this verification code: <strong style="font-size: 24px; color: #667eea;">{{VerificationCode}}</strong>
                </p>
                <p style="color: #555; font-size: 16px; line-height: 1.6; margin-top: 30px;">
                    Best regards,<br>
                    <strong>The {{AppName}} Team</strong>
                </p>
            </div>
            """
    };

    /// <summary>
    /// Gets all default sections.
    /// </summary>
    public static IReadOnlyList<EmailTemplateSection> AllSections => new[]
    {
        DefaultHeader,
        DefaultFooter,
        MinimalHeader,
        MinimalFooter
    };

    /// <summary>
    /// Gets all default templates.
    /// </summary>
    public static IReadOnlyList<EmailTemplate> AllTemplates => new[]
    {
        WelcomeEmail,
        PasswordResetEmail,
        EmailVerification
    };
}
