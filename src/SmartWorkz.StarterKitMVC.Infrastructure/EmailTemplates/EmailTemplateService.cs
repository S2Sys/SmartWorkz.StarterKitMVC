using System.Text.Json;
using System.Text.RegularExpressions;
using SmartWorkz.StarterKitMVC.Application.EmailTemplates;
using SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

namespace SmartWorkz.StarterKitMVC.Infrastructure.EmailTemplates;

/// <summary>
/// Default implementation of email template service.
/// Provides template management, rendering, and import/export functionality.
/// </summary>
public sealed partial class EmailTemplateService : IEmailTemplateService
{
    private readonly IEmailTemplateRepository _repository;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Regex pattern to match placeholders like {{PlaceholderName}}.
    /// </summary>
    [GeneratedRegex(@"\{\{(\w+)\}\}", RegexOptions.Compiled)]
    private static partial Regex PlaceholderRegex();

    public EmailTemplateService(IEmailTemplateRepository repository)
    {
        _repository = repository;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    #region Templates

    public Task<IReadOnlyList<EmailTemplate>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
        => _repository.GetAllTemplatesAsync(null, cancellationToken);

    public Task<EmailTemplate?> GetTemplateByIdAsync(string id, CancellationToken cancellationToken = default)
        => _repository.GetTemplateByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<EmailTemplate>> GetTemplatesByCategoryAsync(string category, CancellationToken cancellationToken = default)
        => _repository.GetTemplatesByCategoryAsync(category, null, cancellationToken);

    public async Task<EmailTemplate> CreateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        if (await _repository.TemplateExistsAsync(template.Id, cancellationToken))
            throw new InvalidOperationException($"Template with ID '{template.Id}' already exists.");

        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = DateTime.UtcNow;
        template.Version = 1;

        return await _repository.SaveTemplateAsync(template, cancellationToken);
    }

    public async Task<EmailTemplate> UpdateTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetTemplateByIdAsync(template.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Template with ID '{template.Id}' not found.");

        if (existing.IsSystem)
            throw new InvalidOperationException("System templates cannot be modified.");

        template.CreatedAt = existing.CreatedAt;
        template.CreatedBy = existing.CreatedBy;
        template.UpdatedAt = DateTime.UtcNow;
        template.Version = existing.Version + 1;

        return await _repository.SaveTemplateAsync(template, cancellationToken);
    }

    public async Task<bool> DeleteTemplateAsync(string id, CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetTemplateByIdAsync(id, cancellationToken);
        if (template?.IsSystem == true)
            throw new InvalidOperationException("System templates cannot be deleted.");

        return await _repository.DeleteTemplateAsync(id, cancellationToken);
    }

    public async Task<EmailTemplate> CloneTemplateAsync(string sourceId, string newId, string newName, CancellationToken cancellationToken = default)
    {
        var source = await _repository.GetTemplateByIdAsync(sourceId, cancellationToken)
            ?? throw new InvalidOperationException($"Source template '{sourceId}' not found.");

        if (await _repository.TemplateExistsAsync(newId, cancellationToken))
            throw new InvalidOperationException($"Template with ID '{newId}' already exists.");

        var clone = new EmailTemplate
        {
            Id = newId,
            Name = newName,
            Description = source.Description,
            Subject = source.Subject,
            HeaderId = source.HeaderId,
            FooterId = source.FooterId,
            BodyContent = source.BodyContent,
            PlainTextContent = source.PlainTextContent,
            Placeholders = source.Placeholders.Select(p => new TemplatePlaceholder
            {
                Key = p.Key,
                DisplayName = p.DisplayName,
                Description = p.Description,
                DefaultValue = p.DefaultValue,
                Type = p.Type,
                IsRequired = p.IsRequired,
                Order = p.Order,
                SampleValue = p.SampleValue
            }).ToList(),
            IsActive = true,
            IsSystem = false,
            Category = source.Category,
            Tags = new List<string>(source.Tags),
            TenantId = source.TenantId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1
        };

        return await _repository.SaveTemplateAsync(clone, cancellationToken);
    }

    #endregion

    #region Sections

    public Task<IReadOnlyList<EmailTemplateSection>> GetAllSectionsAsync(SectionType? type = null, CancellationToken cancellationToken = default)
        => _repository.GetAllSectionsAsync(type, null, cancellationToken);

    public Task<EmailTemplateSection?> GetSectionByIdAsync(string id, CancellationToken cancellationToken = default)
        => _repository.GetSectionByIdAsync(id, cancellationToken);

    public async Task<EmailTemplateSection> CreateSectionAsync(EmailTemplateSection section, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetSectionByIdAsync(section.Id, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException($"Section with ID '{section.Id}' already exists.");

        section.CreatedAt = DateTime.UtcNow;
        section.UpdatedAt = DateTime.UtcNow;

        return await _repository.SaveSectionAsync(section, cancellationToken);
    }

    public async Task<EmailTemplateSection> UpdateSectionAsync(EmailTemplateSection section, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetSectionByIdAsync(section.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Section with ID '{section.Id}' not found.");

        section.CreatedAt = existing.CreatedAt;
        section.CreatedBy = existing.CreatedBy;
        section.UpdatedAt = DateTime.UtcNow;

        return await _repository.SaveSectionAsync(section, cancellationToken);
    }

    public Task<bool> DeleteSectionAsync(string id, CancellationToken cancellationToken = default)
        => _repository.DeleteSectionAsync(id, cancellationToken);

    #endregion

    #region Rendering

    public async Task<EmailTemplateRenderResult> RenderTemplateAsync(
        string templateId,
        IDictionary<string, object> data,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetTemplateByIdAsync(templateId, cancellationToken);
        if (template == null)
            return EmailTemplateRenderResult.Fail($"Template '{templateId}' not found.");

        if (!template.IsActive)
            return EmailTemplateRenderResult.Fail($"Template '{templateId}' is not active.");

        // Validate required placeholders
        var errors = await ValidateTemplateAsync(template, data, cancellationToken);
        if (errors.Count > 0)
            return EmailTemplateRenderResult.Fail(errors.ToArray());

        // Build complete data with system placeholders
        var completeData = BuildCompleteData(data);

        // Render subject
        var subject = ReplacePlaceholders(template.Subject, completeData);

        // Get header and footer
        var headerHtml = string.Empty;
        var footerHtml = string.Empty;

        if (!string.IsNullOrEmpty(template.HeaderId))
        {
            var header = await _repository.GetSectionByIdAsync(template.HeaderId, cancellationToken);
            if (header != null)
                headerHtml = ReplacePlaceholders(header.HtmlContent, completeData);
        }

        if (!string.IsNullOrEmpty(template.FooterId))
        {
            var footer = await _repository.GetSectionByIdAsync(template.FooterId, cancellationToken);
            if (footer != null)
                footerHtml = ReplacePlaceholders(footer.HtmlContent, completeData);
        }

        // Render body
        var bodyHtml = ReplacePlaceholders(template.BodyContent, completeData);

        // Combine all parts
        var fullHtml = $"{headerHtml}{bodyHtml}{footerHtml}";

        // Render plain text if available
        string? plainText = null;
        if (!string.IsNullOrEmpty(template.PlainTextContent))
            plainText = ReplacePlaceholders(template.PlainTextContent, completeData);

        return EmailTemplateRenderResult.Ok(subject, fullHtml, plainText);
    }

    public async Task<EmailTemplateRenderResult> RenderPreviewAsync(
        EmailTemplate template,
        CancellationToken cancellationToken = default)
    {
        // Build sample data from placeholders
        var sampleData = new Dictionary<string, object>();
        
        foreach (var placeholder in template.Placeholders)
        {
            var key = placeholder.Key.Trim('{', '}');
            sampleData[key] = placeholder.SampleValue ?? placeholder.DefaultValue ?? $"[{placeholder.DisplayName}]";
        }

        // Add system placeholders with sample values
        foreach (var sysPh in SystemPlaceholders.All)
        {
            var key = sysPh.Key.Trim('{', '}');
            if (!sampleData.ContainsKey(key))
                sampleData[key] = sysPh.SampleValue ?? $"[{sysPh.DisplayName}]";
        }

        var completeData = BuildCompleteData(sampleData);

        // Render subject
        var subject = ReplacePlaceholders(template.Subject, completeData);

        // Get header and footer
        var headerHtml = string.Empty;
        var footerHtml = string.Empty;

        if (!string.IsNullOrEmpty(template.HeaderId))
        {
            var header = await _repository.GetSectionByIdAsync(template.HeaderId, cancellationToken);
            if (header != null)
                headerHtml = ReplacePlaceholders(header.HtmlContent, completeData);
        }

        if (!string.IsNullOrEmpty(template.FooterId))
        {
            var footer = await _repository.GetSectionByIdAsync(template.FooterId, cancellationToken);
            if (footer != null)
                footerHtml = ReplacePlaceholders(footer.HtmlContent, completeData);
        }

        // Render body
        var bodyHtml = ReplacePlaceholders(template.BodyContent, completeData);

        // Combine all parts
        var fullHtml = $"{headerHtml}{bodyHtml}{footerHtml}";

        return EmailTemplateRenderResult.Ok(subject, fullHtml, null);
    }

    public Task<IReadOnlyList<string>> ValidateTemplateAsync(
        EmailTemplate template,
        IDictionary<string, object> data,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        foreach (var placeholder in template.Placeholders.Where(p => p.IsRequired))
        {
            var key = placeholder.Key.Trim('{', '}');
            if (!data.ContainsKey(key) || data[key] == null || string.IsNullOrEmpty(data[key]?.ToString()))
            {
                if (string.IsNullOrEmpty(placeholder.DefaultValue))
                {
                    errors.Add($"Required placeholder '{placeholder.DisplayName}' ({placeholder.Key}) is missing.");
                }
            }
        }

        return Task.FromResult<IReadOnlyList<string>>(errors);
    }

    private Dictionary<string, object> BuildCompleteData(IDictionary<string, object> data)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        // Add system placeholders first
        foreach (var sysPh in SystemPlaceholders.All)
        {
            var key = sysPh.Key.Trim('{', '}');
            result[key] = sysPh.SampleValue ?? string.Empty;
        }

        // Override with provided data
        foreach (var kvp in data)
        {
            result[kvp.Key.Trim('{', '}')] = kvp.Value;
        }

        return result;
    }

    private string ReplacePlaceholders(string content, IDictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        return PlaceholderRegex().Replace(content, match =>
        {
            var key = match.Groups[1].Value;
            if (data.TryGetValue(key, out var value) && value != null)
                return value.ToString() ?? string.Empty;
            return match.Value; // Keep original if not found
        });
    }

    #endregion

    #region Placeholders

    public IReadOnlyList<TemplatePlaceholder> GetSystemPlaceholders()
        => SystemPlaceholders.All;

    public IReadOnlyList<string> ExtractPlaceholders(string content)
    {
        if (string.IsNullOrEmpty(content))
            return Array.Empty<string>();

        var matches = PlaceholderRegex().Matches(content);
        return matches.Select(m => m.Value).Distinct().ToList();
    }

    #endregion

    #region Import/Export

    public async Task<string> ExportAllAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _repository.GetAllTemplatesAsync(null, cancellationToken);
        var sections = await _repository.GetAllSectionsAsync(null, null, cancellationToken);

        var export = new
        {
            ExportedAt = DateTime.UtcNow,
            Version = "1.0",
            Templates = templates,
            Sections = sections
        };

        return JsonSerializer.Serialize(export, _jsonOptions);
    }

    public async Task<int> ImportAsync(string json, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        var import = JsonSerializer.Deserialize<ImportData>(json, _jsonOptions);
        if (import == null)
            throw new InvalidOperationException("Invalid import data.");

        var count = 0;

        // Import sections first (templates may reference them)
        foreach (var section in import.Sections ?? Enumerable.Empty<EmailTemplateSection>())
        {
            var existing = await _repository.GetSectionByIdAsync(section.Id, cancellationToken);
            if (existing == null || overwrite)
            {
                await _repository.SaveSectionAsync(section, cancellationToken);
                count++;
            }
        }

        // Import templates
        foreach (var template in import.Templates ?? Enumerable.Empty<EmailTemplate>())
        {
            var existing = await _repository.GetTemplateByIdAsync(template.Id, cancellationToken);
            if (existing == null || overwrite)
            {
                await _repository.SaveTemplateAsync(template, cancellationToken);
                count++;
            }
        }

        return count;
    }

    private sealed class ImportData
    {
        public List<EmailTemplate>? Templates { get; set; }
        public List<EmailTemplateSection>? Sections { get; set; }
    }

    #endregion
}
