using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Application.EmailTemplates;
using SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

namespace SmartWorkz.StarterKitMVC.Infrastructure.EmailTemplates;

/// <summary>
/// Dapper repository for email template and section persistence.
/// Implements IEmailTemplateRepository using Master schema stored procedures.
/// Handles Tags JSON serialization and Placeholders child collection hydration.
/// </summary>
public sealed class DapperContentTemplateRepository : IEmailTemplateRepository
{
    private readonly string _connectionString;

    public DapperContentTemplateRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    private IDbConnection GetConnection() => new SqlConnection(_connectionString);

    // ── Templates ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<EmailTemplate>> GetAllTemplatesAsync(string? tenantId = null, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        var templates = await connection.QueryAsync<EmailTemplate>(
            "Master.sp_GetContentTemplatesByTenant",
            new { TenantId = tenantId ?? "DEFAULT", TemplateType = (string?)null, Category = (string?)null },
            commandType: CommandType.StoredProcedure);

        var list = new List<EmailTemplate>();
        foreach (var t in templates)
        {
            t.Placeholders = await GetPlaceholdersAsync(t.Id, connection);
            list.Add(t);
        }

        return list.AsReadOnly();
    }

    public async Task<EmailTemplate?> GetTemplateByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        var template = await connection.QueryFirstOrDefaultAsync<EmailTemplate>(
            "Master.sp_GetContentTemplateById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        if (template is null)
            return null;

        template.Placeholders = await GetPlaceholdersAsync(id, connection);
        return template;
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetTemplatesByCategoryAsync(string category, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        var templates = await connection.QueryAsync<EmailTemplate>(
            "Master.sp_GetContentTemplatesByTenant",
            new { TenantId = tenantId ?? "DEFAULT", TemplateType = (string?)null, Category = category },
            commandType: CommandType.StoredProcedure);

        var list = new List<EmailTemplate>();
        foreach (var t in templates)
        {
            t.Placeholders = await GetPlaceholdersAsync(t.Id, connection);
            list.Add(t);
        }

        return list.AsReadOnly();
    }

    public async Task<EmailTemplate> SaveTemplateAsync(EmailTemplate template, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();

        var tagsJson = template.Tags.Count == 0
            ? null
            : JsonSerializer.Serialize(template.Tags);

        await connection.ExecuteAsync(
            "Master.sp_UpsertContentTemplate",
            new
            {
                template.Id,
                template.Name,
                template.Description,
                TemplateType = "Email",
                template.Subject,
                template.HeaderId,
                template.FooterId,
                template.BodyContent,
                template.PlainTextContent,
                Tags = tagsJson,
                template.Category,
                template.IsActive,
                template.IsSystem,
                template.TenantId,
                template.Version,
                template.CreatedBy,
                template.UpdatedBy
            },
            commandType: CommandType.StoredProcedure);

        // Save placeholders
        var placeholdersJson = JsonSerializer.Serialize(template.Placeholders);
        await connection.ExecuteAsync(
            "Master.sp_ReplaceContentTemplatePlaceholders",
            new { TemplateId = template.Id, Placeholders = placeholdersJson },
            commandType: CommandType.StoredProcedure);

        return template;
    }

    public async Task<bool> DeleteTemplateAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        var affected = await connection.QueryFirstOrDefaultAsync<int>(
            "Master.sp_DeleteContentTemplate",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        return affected > 0;
    }

    public async Task<bool> TemplateExistsAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        var template = await connection.QueryFirstOrDefaultAsync<EmailTemplate>(
            "Master.sp_GetContentTemplateById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        return template is not null;
    }

    // ── Sections ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<EmailTemplateSection>> GetAllSectionsAsync(SectionType? type = null, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        var sections = await connection.QueryAsync<EmailTemplateSection>(
            "Master.sp_GetContentTemplateSectionsByTenant",
            new { TenantId = tenantId ?? "DEFAULT", SectionType = type?.ToString() },
            commandType: CommandType.StoredProcedure);

        return sections.ToList().AsReadOnly();
    }

    public async Task<EmailTemplateSection?> GetSectionByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        // No direct "get by ID" SP — query via GetContentTemplateSectionsByTenant and filter
        var sections = await connection.QueryAsync<EmailTemplateSection>(
            "SELECT * FROM Master.ContentTemplateSections WHERE Id = @Id AND IsDeleted = 0",
            new { Id = id });

        return sections.FirstOrDefault();
    }

    public async Task<EmailTemplateSection?> GetDefaultSectionAsync(SectionType type, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        var section = await connection.QueryFirstOrDefaultAsync<EmailTemplateSection>(
            """
            SELECT * FROM Master.ContentTemplateSections
            WHERE IsDefault = 1 AND SectionType = @SectionType
              AND (TenantId = @TenantId OR (TenantId IS NULL AND @TenantId IS NULL))
              AND IsActive = 1 AND IsDeleted = 0
            """,
            new { SectionType = type.ToString(), TenantId = tenantId });

        return section;
    }

    public async Task<EmailTemplateSection> SaveSectionAsync(EmailTemplateSection section, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();

        await connection.ExecuteAsync(
            "Master.sp_UpsertContentTemplateSection",
            new
            {
                section.Id,
                section.Name,
                SectionType = section.SectionType,
                section.HtmlContent,
                section.IsDefault,
                section.IsActive,
                section.TenantId,
                section.CreatedBy,
                section.UpdatedBy
            },
            commandType: CommandType.StoredProcedure);

        return section;
    }

    public async Task<bool> DeleteSectionAsync(string id, CancellationToken cancellationToken = default)
    {
        using var connection = GetConnection();
        var affected = await connection.ExecuteAsync(
            "UPDATE Master.ContentTemplateSections SET IsDeleted = 1, UpdatedAt = GETUTCDATE() WHERE Id = @Id",
            new { Id = id });

        return affected > 0;
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<List<TemplatePlaceholder>> GetPlaceholdersAsync(string templateId, IDbConnection connection)
    {
        var placeholders = await connection.QueryAsync<TemplatePlaceholder>(
            "Master.sp_GetContentTemplatePlaceholders",
            new { TemplateId = templateId },
            commandType: CommandType.StoredProcedure);

        return placeholders.ToList();
    }
}
