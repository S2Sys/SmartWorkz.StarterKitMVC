namespace SmartWorkz.Mobile;

/// <summary>
/// Defines location accuracy levels balancing power consumption and precision.
/// </summary>
public enum LocationAccuracy
{
    /// <summary>Lowest accuracy, best battery life (~5000m error).</summary>
    Lowest = 0,

    /// <summary>Low accuracy (~500m error).</summary>
    Low = 1,

    /// <summary>Medium accuracy (~100m error).</summary>
    Medium = 2,

    /// <summary>High accuracy (~10m error).</summary>
    High = 3,

    /// <summary>Highest accuracy (~1m error), worst battery life.</summary>
    Best = 4
}
