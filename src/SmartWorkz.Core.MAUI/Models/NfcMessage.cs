namespace SmartWorkz.Mobile;

/// <summary>
/// Represents an NFC (Near Field Communication) message read from a tag or device.
/// Contains the message type, payload data, and optional parsed URI or text content.
/// </summary>
public sealed record NfcMessage(
    /// <summary>
    /// The type of NFC message (e.g., "URI", "TEXT", "MIME_TYPE").
    /// Indicates the format and interpretation of the Payload and optional Uri/Text properties.
    /// </summary>
    string MessageType,

    /// <summary>
    /// The raw payload data from the NFC tag, encoded as a string.
    /// The format and content depend on the MessageType.
    /// </summary>
    string Payload,

    /// <summary>
    /// The timestamp when the NFC message was detected and read.
    /// Useful for tracking when the message was captured.
    /// </summary>
    DateTime DetectedAt,

    /// <summary>
    /// The parsed URI content if the message type indicates a URI.
    /// Null if the message does not contain URI data.
    /// Check IsUri property before accessing.
    /// </summary>
    string? Uri = null,

    /// <summary>
    /// The parsed text content if the message type indicates text data.
    /// Null if the message does not contain text data.
    /// Check IsText property before accessing.
    /// </summary>
    string? Text = null)
{
    /// <summary>
    /// Gets a value indicating whether this message contains URI data.
    /// True if the Uri property is not null, false otherwise.
    /// </summary>
    public bool IsUri => Uri is not null;

    /// <summary>
    /// Gets a value indicating whether this message contains text data.
    /// True if the Text property is not null, false otherwise.
    /// </summary>
    public bool IsText => Text is not null;
}
