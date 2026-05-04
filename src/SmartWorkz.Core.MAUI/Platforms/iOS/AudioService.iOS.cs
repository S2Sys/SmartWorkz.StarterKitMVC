namespace SmartWorkz.Mobile;

#if __IOS__

public partial class AudioService
{
    private Dictionary<string, object> _audioPlayers = new();

    private partial async Task<string> PlayAsyncPlatform(string filePath, CancellationToken ct)
    {
        var audioId = Guid.NewGuid().ToString();
        _audioPlayers[audioId] = new object(); // Placeholder for AVAudioPlayer
        return audioId;
    }

    private partial async Task StopAsyncPlatform(string audioId, CancellationToken ct) => _audioPlayers.Remove(audioId);
    private partial async Task PauseAsyncPlatform(string audioId, CancellationToken ct) { }
    private partial async Task ResumeAsyncPlatform(string audioId, CancellationToken ct) { }
    private partial async Task<string> StartRecordingAsyncPlatform(string outputPath, CancellationToken ct) => Guid.NewGuid().ToString();
    private partial async Task StopRecordingAsyncPlatform(string recordingId, CancellationToken ct) { }
    private partial async Task SetVolumeAsyncPlatform(float volume, CancellationToken ct) { }
    private partial async Task<bool> IsMicrophoneAvailableAsyncPlatform(CancellationToken ct) => true;
}

#endif
