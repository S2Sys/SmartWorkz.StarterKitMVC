namespace SmartWorkz.Mobile;

#if WINDOWS

public partial class AudioService
{
    private partial async Task<string> PlayAsyncPlatform(string filePath, CancellationToken ct) => string.Empty;
    private partial async Task StopAsyncPlatform(string audioId, CancellationToken ct) { }
    private partial async Task PauseAsyncPlatform(string audioId, CancellationToken ct) { }
    private partial async Task ResumeAsyncPlatform(string audioId, CancellationToken ct) { }
    private partial async Task<string> StartRecordingAsyncPlatform(string outputPath, CancellationToken ct) => string.Empty;
    private partial async Task StopRecordingAsyncPlatform(string recordingId, CancellationToken ct) { }
    private partial async Task SetVolumeAsyncPlatform(float volume, CancellationToken ct) { }
    private partial async Task<bool> IsMicrophoneAvailableAsyncPlatform(CancellationToken ct) => false;
}

#endif
