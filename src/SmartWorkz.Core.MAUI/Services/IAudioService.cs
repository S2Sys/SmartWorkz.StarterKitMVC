namespace SmartWorkz.Mobile;

/// <summary>
/// Service for audio playback, recording, and system sound control on mobile devices.
/// Handles audio file management, volume control, and playback state.
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// Plays an audio file from the given file path.
    /// </summary>
    /// <param name="filePath">Path to audio file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Audio player ID for tracking and control</returns>
    Task<string> PlayAsync(string filePath, CancellationToken ct = default);

    /// <summary>
    /// Stops playback of the specified audio.
    /// </summary>
    /// <param name="audioId">ID of audio to stop</param>
    /// <param name="ct">Cancellation token</param>
    Task StopAsync(string audioId, CancellationToken ct = default);

    /// <summary>
    /// Pauses the specified audio playback.
    /// </summary>
    /// <param name="audioId">ID of audio to pause</param>
    /// <param name="ct">Cancellation token</param>
    Task PauseAsync(string audioId, CancellationToken ct = default);

    /// <summary>
    /// Resumes paused audio playback.
    /// </summary>
    /// <param name="audioId">ID of audio to resume</param>
    /// <param name="ct">Cancellation token</param>
    Task ResumeAsync(string audioId, CancellationToken ct = default);

    /// <summary>
    /// Starts recording audio from the device microphone.
    /// </summary>
    /// <param name="outputPath">Path to save recorded audio file</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Recording ID for tracking and control</returns>
    Task<string> StartRecordingAsync(string outputPath, CancellationToken ct = default);

    /// <summary>
    /// Stops the current audio recording.
    /// </summary>
    /// <param name="recordingId">ID of recording to stop</param>
    /// <param name="ct">Cancellation token</param>
    Task StopRecordingAsync(string recordingId, CancellationToken ct = default);

    /// <summary>
    /// Sets the playback volume (0.0 to 1.0).
    /// </summary>
    /// <param name="volume">Volume level from 0.0 (mute) to 1.0 (max)</param>
    /// <param name="ct">Cancellation token</param>
    Task SetVolumeAsync(float volume, CancellationToken ct = default);

    /// <summary>
    /// Gets the current playback volume (0.0 to 1.0).
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current volume level</returns>
    Task<float> GetVolumeAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks if microphone permission is granted.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if permission granted</returns>
    Task<bool> IsMicrophoneAvailableAsync(CancellationToken ct = default);
}
