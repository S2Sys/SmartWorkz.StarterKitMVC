using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SmartWorkz.StarterKitMVC.Application.Localization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

public class DapperTranslationRepository : ITranslationRepository
{
    private readonly string _connectionString;

    public DapperTranslationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured.");
    }

    public async Task<IEnumerable<TranslationEntry>> GetAllAsync(string tenantId, string locale)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<TranslationEntry>(
            "Shared.sp_GetTranslations",
            new { TenantId = tenantId, Locale = locale },
            commandType: CommandType.StoredProcedure);
    }
}
