namespace SmartWorkz.Core.Mobile;

internal sealed record TelemetryEventPayload(
    string EventName,
    string EventType,
    string? UserId,
    Dictionary<string, object>? Properties,
    Dictionary<string, string>? UserProperties,
    string Platform,
    string DeviceId,
    DateTimeOffset OccurredAt);
