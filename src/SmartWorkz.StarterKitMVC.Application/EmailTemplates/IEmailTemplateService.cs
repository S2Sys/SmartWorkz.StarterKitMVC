using SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

namespace SmartWorkz.StarterKitMVC.Application.EmailTemplates;

/// <summary>
/// Service interface for email template management and rendering.
/// </summary>
/// <example>
/// <code>
/// // Render a template with data
/// var result = await templateService.RenderTemplateAsync("welcome-email", new Dictionary&lt;string, object&gt;
/// {
///     ["UserName"] = "John Doe",
///     ["UserEmail"] = "john@example.com"
/// });
/// 
/// if (result.Success)
/// {
///     await emailService.SendAsync(recipient, result.Subject, result.HtmlBody);
/// }
/// </code>
/// </example>
public interface IEmailTemplateService
{
    #region Templates
    
    /// <summary>
    /// Gets all email templates.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an email template by its ID.
    /// </summary>
    Task<EmailTemplate?> GetTemplateByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets email templates by category.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetTemplatesByCategoryAsync(string category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new email template.
    /// </summary>
    Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing email template.
    /// </summary>
    Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an email template.
    /// </summary>
    Task<bool> DeleteTemplateAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clones an existing template with a new ID and name.
    /// </summary>
    Task<EmailTemplate> CloneTemplateAsync(string sourceId, string newId, string newName, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Sections
    
    /// <summary>
    /// Gets all template sections (headers and footers).
    /// </summary>
    Task<IReadOnlyList<EmailTemplateSection>> GetAllSectionsAsync(SectionType? type = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a template section by its ID.
    /// </summary>
    Task<EmailTemplateSection?> GetSectionByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new template section.
    /// </summary>
    Task<EmailTemplateSection> CreateSectionAsync(EmailTemplateSection section, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing template section.
    /// </summary>
    Task<EmailTemplateSection> UpdateSectionAsync(EmailTemplateSection section, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a template section.
    /// </summary>
    Task<bool> DeleteSectionAsync(string id, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Rendering
    
    /// <summary>
    /// Renders an email template with the provided data.
    /// </summary>
    /// <param name="templateId">The template ID to render.</param>
    /// <param name="data">Dictionary of placeholder values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The rendered email result.</returns>
    Task<EmailTemplateRenderResult> RenderTemplateAsync(
        string templateId, 
        IDictionary<string, object> data, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Renders a template preview with sample data.
    /// </summary>
    Task<EmailTemplateRenderResult> RenderPreviewAsync(
        EmailTemplate template,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates a template for missing required placeholders.
    /// </summary>
    Task<IReadOnlyList<string>> ValidateTemplateAsync(
        EmailTemplate template,
        IDictionary<string, object> data,
        CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Placeholders
    
    /// <summary>
    /// Gets all system placeholders.
    /// </summary>
    IReadOnlyList<TemplatePlaceholder> GetSystemPlaceholders();
    
    /// <summary>
    /// Extracts placeholder keys from content.
    /// </summary>
    IReadOnlyList<string> ExtractPlaceholders(string content);
    
    #endregion
    
    #region Import/Export
    
    /// <summary>
    /// Exports all templates and sections to JSON.
    /// </summary>
    Task<string> ExportAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Imports templates and sections from JSON.
    /// </summary>
    Task<int> ImportAsync(string json, bool overwrite = false, CancellationToken cancellationToken = default);
    
    #endregion
}
