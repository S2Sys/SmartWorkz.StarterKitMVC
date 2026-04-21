namespace SmartWorkz.Core.Mobile;

#if !WINDOWS
using SQLite;

public class LocalStorageService : ILocalStorageService
{
    private readonly ILogger _logger;
    private readonly AsyncLazy<SQLiteAsyncConnection> _database;

    private class StorageEntry
    {
        [PrimaryKey] public string Key { get; set; } = "";
        public string ValueJson { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string? ExpiresAt { get; set; }
    }

    public LocalStorageService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
        _database = new AsyncLazy<SQLiteAsyncConnection>(InitializeDatabaseAsync);
    }

    private async Task<SQLiteAsyncConnection> InitializeDatabaseAsync()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "smartworkz_local.db");
        var connection = new SQLiteAsyncConnection(dbPath);
        await connection.CreateTableAsync<StorageEntry>();
        _logger.LogInformation("LocalStorage database initialized");
        return connection;
    }

    public async Task<Result> SaveAsync<T>(string key, T value, CancellationToken ct = default)
    {
        try
        {
            Guard.NotEmpty(key, nameof(key));
            if (value == null)
                return Result.Fail(new Error("VALIDATION.NULL_VALUE", $"{nameof(value)} cannot be null"));

            var db = await _database.Value;
            var json = JsonSerializer.Serialize(value);
            var entry = new StorageEntry
            {
                Key = key,
                ValueJson = json,
                TypeName = typeof(T).FullName ?? typeof(T).Name
            };
            await db.InsertOrReplaceAsync(entry);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("LocalStorage.SaveAsync failed", ex);
            return Result.Fail(new Error("STORAGE.SQLITE_ERROR", ex.Message));
        }
    }

    public async Task<Result<T>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            Guard.NotEmpty(key, nameof(key));

            var db = await _database.Value;
            var entry = await db.FindAsync<StorageEntry>(key);

            if (entry == null)
                return Result.Fail<T>(new Error("CACHE.MISS", $"Key {key} not found"));

            if (entry.ExpiresAt != null && DateTime.Parse(entry.ExpiresAt) <= DateTime.UtcNow)
            {
                await db.DeleteAsync(entry);
                return Result.Fail<T>(new Error("CACHE.MISS", $"Key {key} expired"));
            }

            var value = JsonSerializer.Deserialize<T>(entry.ValueJson);
            return value == null
                ? Result.Fail<T>(new Error("SERIALIZATION.FAILED", "Deserialization returned null"))
                : Result.Ok(value);
        }
        catch (Exception ex)
        {
            _logger.LogError("LocalStorage.GetAsync failed", ex);
            return Result.Fail<T>(new Error("STORAGE.SQLITE_ERROR", ex.Message));
        }
    }

    public async Task<Result> DeleteAsync(string key, CancellationToken ct = default)
    {
        try
        {
            Guard.NotEmpty(key, nameof(key));

            var db = await _database.Value;
            await db.DeleteAsync<StorageEntry>(key);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("LocalStorage.DeleteAsync failed", ex);
            return Result.Fail(new Error("STORAGE.SQLITE_ERROR", ex.Message));
        }
    }

    public async Task<Result<IEnumerable<T>>> GetAllAsync<T>(CancellationToken ct = default)
    {
        try
        {
            var db = await _database.Value;
            var typeName = typeof(T).FullName ?? typeof(T).Name;
            var entries = await db.QueryAsync<StorageEntry>("SELECT * FROM StorageEntry WHERE TypeName = ?", typeName);

            var results = new List<T>();
            foreach (var entry in entries)
            {
                if (entry.ExpiresAt != null && DateTime.Parse(entry.ExpiresAt) <= DateTime.UtcNow)
                {
                    await db.DeleteAsync(entry);
                    continue;
                }

                var value = JsonSerializer.Deserialize<T>(entry.ValueJson);
                if (value != null)
                    results.Add(value);
            }

            return Result.Ok((IEnumerable<T>)results);
        }
        catch (Exception ex)
        {
            _logger.LogError("LocalStorage.GetAllAsync failed", ex);
            return Result.Fail<IEnumerable<T>>(new Error("STORAGE.SQLITE_ERROR", ex.Message));
        }
    }

    public async Task<Result<IEnumerable<T>>> GetAllByPrefixAsync<T>(string keyPrefix, CancellationToken ct = default)
    {
        try
        {
            Guard.NotEmpty(keyPrefix, nameof(keyPrefix));

            var db = await _database.Value;
            var typeName = typeof(T).FullName ?? typeof(T).Name;
            var entries = await db.QueryAsync<StorageEntry>(
                "SELECT * FROM StorageEntry WHERE TypeName = ? AND Key LIKE ?",
                typeName,
                keyPrefix + "%"
            );

            var results = new List<T>();
            foreach (var entry in entries)
            {
                if (entry.ExpiresAt != null && DateTime.Parse(entry.ExpiresAt) <= DateTime.UtcNow)
                {
                    await db.DeleteAsync(entry);
                    continue;
                }

                var value = JsonSerializer.Deserialize<T>(entry.ValueJson);
                if (value != null)
                    results.Add(value);
            }

            return Result.Ok((IEnumerable<T>)results);
        }
        catch (Exception ex)
        {
            _logger.LogError("LocalStorage.GetAllByPrefixAsync failed", ex);
            return Result.Fail<IEnumerable<T>>(new Error("STORAGE.SQLITE_ERROR", ex.Message));
        }
    }

    public async Task<Result> ClearAsync(CancellationToken ct = default)
    {
        try
        {
            var db = await _database.Value;
            await db.DeleteAllAsync<StorageEntry>();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("LocalStorage.ClearAsync failed", ex);
            return Result.Fail(new Error("STORAGE.SQLITE_ERROR", ex.Message));
        }
    }
}

internal class AsyncLazy<T>
{
    private readonly Lazy<Task<T>> _instance;

    public AsyncLazy(Func<Task<T>> factory)
    {
        _instance = new Lazy<Task<T>>(() => factory());
    }

    public Task<T> Value => _instance.Value;
}
#else

public class LocalStorageService : ILocalStorageService
{
    private readonly ILogger _logger;

    public LocalStorageService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<Result> SaveAsync<T>(string key, T value, CancellationToken ct = default)
    {
        _logger.LogWarning("LocalStorage not available on Windows platform");
        return Result.Fail(new Error("STORAGE.PLATFORM_UNSUPPORTED", "LocalStorage not available on Windows"));
    }

    public async Task<Result<T>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        return Result.Fail<T>(new Error("STORAGE.PLATFORM_UNSUPPORTED", "LocalStorage not available on Windows"));
    }

    public async Task<Result> DeleteAsync(string key, CancellationToken ct = default)
    {
        return Result.Fail(new Error("STORAGE.PLATFORM_UNSUPPORTED", "LocalStorage not available on Windows"));
    }

    public async Task<Result<IEnumerable<T>>> GetAllAsync<T>(CancellationToken ct = default)
    {
        return Result.Fail<IEnumerable<T>>(new Error("STORAGE.PLATFORM_UNSUPPORTED", "LocalStorage not available on Windows"));
    }

    public async Task<Result<IEnumerable<T>>> GetAllByPrefixAsync<T>(string keyPrefix, CancellationToken ct = default)
    {
        return Result.Fail<IEnumerable<T>>(new Error("STORAGE.PLATFORM_UNSUPPORTED", "LocalStorage not available on Windows"));
    }

    public async Task<Result> ClearAsync(CancellationToken ct = default)
    {
        return Result.Fail(new Error("STORAGE.PLATFORM_UNSUPPORTED", "LocalStorage not available on Windows"));
    }
}
#endif
