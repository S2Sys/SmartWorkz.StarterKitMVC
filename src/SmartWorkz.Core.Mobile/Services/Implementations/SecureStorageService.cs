namespace SmartWorkz.Core.Mobile;

#if !WINDOWS
public class SecureStorageService : ISecureStorageService
{
    private readonly ILogger _logger;

    public SecureStorageService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<Result> SetAsync(string key, string value, CancellationToken ct = default)
    {
        try
        {
            Guard.NotEmpty(key, nameof(key));
            Guard.NotEmpty(value, nameof(value));

            await SecureStorage.Default.SetAsync(key, value);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("SecureStorage.SetAsync failed", ex);
            return Result.Fail(new Error("SECURE_STORAGE.ACCESS_DENIED", ex.Message));
        }
    }

    public async Task<Result<string>> GetAsync(string key, CancellationToken ct = default)
    {
        try
        {
            Guard.NotEmpty(key, nameof(key));

            var value = await SecureStorage.Default.GetAsync(key);
            return value == null
                ? Result.Fail<string>(new Error("SECURE_STORAGE.NOT_FOUND", $"Key {key} not found"))
                : Result.Ok(value);
        }
        catch (Exception ex)
        {
            _logger.LogError("SecureStorage.GetAsync failed", ex);
            return Result.Fail<string>(new Error("SECURE_STORAGE.ACCESS_DENIED", ex.Message));
        }
    }

    public async Task<Result> DeleteAsync(string key, CancellationToken ct = default)
    {
        try
        {
            Guard.NotEmpty(key, nameof(key));

            SecureStorage.Default.Remove(key);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("SecureStorage.Remove failed", ex);
            return Result.Fail(new Error("SECURE_STORAGE.ACCESS_DENIED", ex.Message));
        }
    }

    public async Task<Result> ClearAsync(CancellationToken ct = default)
    {
        try
        {
            SecureStorage.Default.RemoveAll();
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError("SecureStorage.RemoveAll failed", ex);
            return Result.Fail(new Error("SECURE_STORAGE.ACCESS_DENIED", ex.Message));
        }
    }
}
#else

public class SecureStorageService : ISecureStorageService
{
    private readonly ILogger _logger;

    public SecureStorageService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<Result> SetAsync(string key, string value, CancellationToken ct = default)
    {
        _logger.LogWarning("SecureStorage not available on Windows platform");
        return Result.Fail(new Error("SECURE_STORAGE.PLATFORM_UNSUPPORTED", "SecureStorage not available on Windows"));
    }

    public async Task<Result<string>> GetAsync(string key, CancellationToken ct = default)
    {
        return Result.Fail<string>(new Error("SECURE_STORAGE.PLATFORM_UNSUPPORTED", "SecureStorage not available on Windows"));
    }

    public async Task<Result> DeleteAsync(string key, CancellationToken ct = default)
    {
        return Result.Fail(new Error("SECURE_STORAGE.PLATFORM_UNSUPPORTED", "SecureStorage not available on Windows"));
    }

    public async Task<Result> ClearAsync(CancellationToken ct = default)
    {
        return Result.Fail(new Error("SECURE_STORAGE.PLATFORM_UNSUPPORTED", "SecureStorage not available on Windows"));
    }
}
#endif
