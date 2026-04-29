namespace SmartWorkz.Mobile;

/// <summary>Represents a captured or selected video.</summary>
public class Video
{
    /// <summary>Gets or sets the file path where video is saved.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the video duration.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Gets or sets video dimensions (WIDTHxHEIGHT).</summary>
    public string Resolution { get; set; } = string.Empty;

    /// <summary>Gets or sets capture timestamp.</summary>
    public DateTime CaptureTime { get; set; } = DateTime.UtcNow;
}
