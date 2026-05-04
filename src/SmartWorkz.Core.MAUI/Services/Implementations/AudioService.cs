namespace SmartWorkz.Mobile;

using ILogger = Microsoft.Extensions.Logging.ILogger;

#if !WINDOWS
public partial class AudioService : IAudioService
#else
public class AudioService : IAudioService
#endif
{
    private readonly ILogger _logger;
    private float _volume = 1.0f;

    public AudioService(ILogger logger)
    {
        _logger = Guard.NotNull(logger, nameof(logger));
    }

    public async Task<string> PlayAsync(string filePath, CancellationToken ct = default)
    {
        Guard.NotEmpty(filePath, nameof(filePath));
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogWarning("Audio playback not available on Windows");
        return string.Empty;
        #else
        try
        {
            return await PlayAsyncPlatform(filePath, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to play audio");
            return string.Empty;
        }
        #endif
    }

    public async Task StopAsync(string audioId, CancellationToken ct = default)
    {
        Guard.NotEmpty(audioId, nameof(audioId));
        ct.ThrowIfCancellationRequested();

        #if !WINDOWS
        try
        {
            await StopAsyncPlatform(audioId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop audio");
        }
        #endif
    }

    public async Task PauseAsync(string audioId, CancellationToken ct = default)
    {
        Guard.NotEmpty(audioId, nameof(audioId));
        ct.ThrowIfCancellationRequested();

        #if !WINDOWS
        try
        {
            await PauseAsyncPlatform(audioId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pause audio");
        }
        #endif
    }

    public async Task ResumeAsync(string audioId, CancellationToken ct = default)
    {
        Guard.NotEmpty(audioId, nameof(audioId));
        ct.ThrowIfCancellationRequested();

        #if !WINDOWS
        try
        {
            await ResumeAsyncPlatform(audioId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume audio");
        }
        #endif
    }

    public async Task<string> StartRecordingAsync(string outputPath, CancellationToken ct = default)
    {
        Guard.NotEmpty(outputPath, nameof(outputPath));
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        _logger.LogWarning("Audio recording not available on Windows");
        return string.Empty;
        #else
        try
        {
            return await StartRecordingAsyncPlatform(outputPath, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start recording");
            return string.Empty;
        }
        #endif
    }

    public async Task StopRecordingAsync(string recordingId, CancellationToken ct = default)
    {
        Guard.NotEmpty(recordingId, nameof(recordingId));
        ct.ThrowIfCancellationRequested();

        #if !WINDOWS
        try
        {
            await StopRecordingAsyncPlatform(recordingId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop recording");
        }
        #endif
    }

    public async Task SetVolumeAsync(float volume, CancellationToken ct = default)
    {
        if (volume < 0f || volume > 1f)
            throw new ArgumentException("Volume must be between 0.0 and 1.0", nameof(volume));
        ct.ThrowIfCancellationRequested();

        _volume = volume;

        #if !WINDOWS
        try
        {
            await SetVolumeAsyncPlatform(volume, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set volume");
        }
        #endif
    }

    public async Task<float> GetVolumeAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return _volume;
    }

    public async Task<bool> IsMicrophoneAvailableAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        #if WINDOWS
        return false;
        #else
        try
        {
            return await IsMicrophoneAvailableAsyncPlatform(ct);
        }
        catch
        {
            return false;
        }
        #endif
    }

    #if !WINDOWS
    private partial Task<string> PlayAsyncPlatform(string filePath, CancellationToken ct);
    private partial Task StopAsyncPlatform(string audioId, CancellationToken ct);
    private partial Task PauseAsyncPlatform(string audioId, CancellationToken ct);
    private partial Task ResumeAsyncPlatform(string audioId, CancellationToken ct);
    private partial Task<string> StartRecordingAsyncPlatform(string outputPath, CancellationToken ct);
    private partial Task StopRecordingAsyncPlatform(string recordingId, CancellationToken ct);
    private partial Task SetVolumeAsyncPlatform(float volume, CancellationToken ct);
    private partial Task<bool> IsMicrophoneAvailableAsyncPlatform(CancellationToken ct);
    #endif
}
