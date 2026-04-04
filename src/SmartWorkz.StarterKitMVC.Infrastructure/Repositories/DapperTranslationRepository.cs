using System.Data;
using Dapper;
using SmartWorkz.StarterKitMVC.Application.Localization;

namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

public class DapperTranslationRepository : ITranslationRepository
{
    private readonly IDbConnection _connection;

    public DapperTranslationRepository(IDbConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    public async Task<IEnumerable<TranslationEntry>> GetAllAsync(string tenantId, string locale)
    {
        return await _connection.QueryAsync<TranslationEntry>(
            "Shared.sp_GetTranslations",
            new { TenantId = tenantId, Locale = locale },
            commandType: CommandType.StoredProcedure);
    }
}
