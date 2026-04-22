namespace SmartWorkz.Mobile;

public sealed record AccelerometerReading(
    double X,
    double Y,
    double Z,
    DateTime Timestamp)
{
    /// <summary>Calculates the magnitude (length) of the acceleration vector.</summary>
    public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>Gets a specific axis component value.</summary>
    public double GetComponent(Axis axis) => axis switch
    {
        Axis.X => X,
        Axis.Y => Y,
        Axis.Z => Z,
        _ => 0.0
    };

    /// <summary>Calculates the difference between two readings.</summary>
    public AccelerometerReading Subtract(AccelerometerReading other) =>
        new(X - other.X, Y - other.Y, Z - other.Z, Timestamp);
}

/// <summary>Enumeration of accelerometer axes.</summary>
public enum Axis { X, Y, Z }
