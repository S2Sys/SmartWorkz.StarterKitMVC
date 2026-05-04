namespace SmartWorkz.Mobile;

/// <summary>Represents a captured or selected photo.</summary>
public class Photo
{
    /// <summary>Gets or sets the file path where photo is saved.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Gets or sets raw image bytes.</summary>
    public byte[]? ImageBytes { get; set; }

    /// <summary>Gets or sets the Base64-encoded image for transmission.</summary>
    public string? Base64 => ImageBytes != null ? Convert.ToBase64String(ImageBytes) : null;

    /// <summary>Gets or sets width in pixels.</summary>
    public int Width { get; set; }

    /// <summary>Gets or sets height in pixels.</summary>
    public int Height { get; set; }

    /// <summary>Gets or sets capture timestamp.</summary>
    public DateTime CaptureTime { get; set; } = DateTime.UtcNow;
}
