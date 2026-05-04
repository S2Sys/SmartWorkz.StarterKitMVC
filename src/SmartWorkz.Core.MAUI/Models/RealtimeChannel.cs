namespace SmartWorkz.Mobile.Models;

/// <summary>
/// Available real-time communication channels.
/// </summary>
public enum RealtimeChannel
{
    Orders = 1,
    Notifications = 2,
    UserPresence = 3,
    ChatMessages = 4,
    LocationUpdates = 5,
    SystemAlerts = 6,
    CustomChannel = 99,
}
