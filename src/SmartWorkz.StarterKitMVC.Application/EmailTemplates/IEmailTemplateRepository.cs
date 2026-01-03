using SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

namespace SmartWorkz.StarterKitMVC.Application.EmailTemplates;

/// <summary>
/// Repository interface for email template persistence operations.
/// </summary>
public interface IEmailTemplateRepository
{
    #region Templates
    
    /// <summary>
    /// Gets all email templates, optionally filtered by tenant.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetAllTemplatesAsync(string? tenantId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an email template by its ID.
    /// </summary>
    Task<EmailTemplate?> GetTemplateByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets email templates by category.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetTemplatesByCategoryAsync(string category, string? tenantId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves an email template (creates or updates).
    /// </summary>
    Task<EmailTemplate> SaveTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an email template by its ID.
    /// </summary>
    Task<bool> DeleteTemplateAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a template with the given ID exists.
    /// </summary>
    Task<bool> TemplateExistsAsync(string id, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Sections
    
    /// <summary>
    /// Gets all template sections, optionally filtered by type and tenant.
    /// </summary>
    Task<IReadOnlyList<EmailTemplateSection>> GetAllSectionsAsync(SectionType? type = null, string? tenantId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a template section by its ID.
    /// </summary>
    Task<EmailTemplateSection?> GetSectionByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the default section for a given type.
    /// </summary>
    Task<EmailTemplateSection?> GetDefaultSectionAsync(SectionType type, string? tenantId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Saves a template section (creates or updates).
    /// </summary>
    Task<EmailTemplateSection> SaveSectionAsync(EmailTemplateSection section, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a template section by its ID.
    /// </summary>
    Task<bool> DeleteSectionAsync(string id, CancellationToken cancellationToken = default);
    
    #endregion
}
