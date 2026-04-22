// src/SmartWorkz.Core.Mobile/Models/NfcMessage.cs
namespace SmartWorkz.Mobile;

public sealed record NfcMessage(
    string MessageType,
    string Payload,
    DateTime DetectedAt,
    string? Uri = null,
    string? Text = null)
{
    public bool IsUri => Uri is not null;
    public bool IsText => Text is not null;
}
