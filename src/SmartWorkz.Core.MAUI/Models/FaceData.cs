using System.Text.Json;
using SmartWorkz.Shared;

namespace SmartWorkz.Mobile;

/// <summary>
/// Represents a detected human face with biometric data and ML model predictions.
/// </summary>
/// <remarks>
/// This record encapsulates comprehensive face detection and recognition data including
/// spatial coordinates, facial landmarks, recognition results, and optional facial attributes.
/// All validation is performed during record construction. The bounding box is stored as JSON
/// for flexible deserialization, and facial landmarks must contain at least 5 key points.
/// </remarks>
public sealed record FaceData(
    string FaceId,
    float Confidence,
    string BoundingBox,
    Dictionary<string, (float X, float Y)> LandmarkPoints,
    float Orientation,
    int FaceWidth,
    int FaceHeight,
    string? RecognizedIdentity = null,
    float? IdentityConfidence = null,
    bool? IsSmiling = null,
    bool? EyesOpen = null,
    Dictionary<string, string>? FaceAttributes = null,
    DateTime? DetectedAt = null,
    Dictionary<string, string>? Metadata = null)
{
    /// <summary>
    /// Gets the unique identifier for the detected face in UUID format.
    /// Must be a valid UUID: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
    /// </summary>
    public string FaceId { get; } = ValidateFaceId(FaceId);

    /// <summary>
    /// Gets the detection confidence score, ranging from 0.0 (poor) to 1.0 (high confidence).
    /// Represents the ML model's confidence in the face detection.
    /// </summary>
    public float Confidence { get; } = ValidateConfidence(Confidence);

    /// <summary>
    /// Gets the JSON-serialized bounding box containing x, y, width, and height coordinates
    /// in image pixel space. Must be a valid JSON string with numeric properties.
    /// </summary>
    public string BoundingBox { get; } = ValidateBoundingBox(BoundingBox);

    /// <summary>
    /// Gets the dictionary of facial landmark points with keys (e.g., left_eye, right_eye, nose).
    /// Must contain at least 5 entries with (X, Y) coordinate tuples.
    /// </summary>
    public Dictionary<string, (float X, float Y)> LandmarkPoints { get; } = ValidateLandmarkPoints(LandmarkPoints);

    /// <summary>
    /// Gets the head rotation in degrees, ranging from -180 (full left) to 180 (full right).
    /// Represents the out-of-plane rotation of the detected face.
    /// </summary>
    public float Orientation { get; } = ValidateOrientation(Orientation);

    /// <summary>
    /// Gets the pixel width of the detected face bounding box.
    /// Must be at least 10 pixels.
    /// </summary>
    public int FaceWidth { get; } = ValidateFaceWidth(FaceWidth);

    /// <summary>
    /// Gets the pixel height of the detected face bounding box.
    /// Must be at least 10 pixels.
    /// </summary>
    public int FaceHeight { get; } = ValidateFaceHeight(FaceHeight);

    /// <summary>
    /// Gets the identified person's name if this face has been recognized.
    /// Null if the face remains unidentified.
    /// </summary>
    public string? RecognizedIdentity { get; } = RecognizedIdentity;

    /// <summary>
    /// Gets the recognition confidence score if a match was found, ranging from 0.0 to 1.0.
    /// Null if RecognizedIdentity is null. Only valid if face has been matched to a known person.
    /// </summary>
    public float? IdentityConfidence { get; } = ValidateIdentityConfidence(IdentityConfidence);

    /// <summary>
    /// Gets a value indicating whether the detected face appears to be smiling.
    /// Null if smile detection was not performed or is unavailable.
    /// </summary>
    public bool? IsSmiling { get; } = IsSmiling;

    /// <summary>
    /// Gets a value indicating whether the detected face has eyes open.
    /// Null if eye state detection was not performed or is unavailable.
    /// </summary>
    public bool? EyesOpen { get; } = EyesOpen;

    /// <summary>
    /// Gets optional facial attributes such as age, gender, and ethnicity estimates.
    /// If null is provided, defaults to an empty dictionary.
    /// </summary>
    public Dictionary<string, string> FaceAttributes { get; } = FaceAttributes ?? new Dictionary<string, string>();

    /// <summary>
    /// Gets the timestamp when this face was detected.
    /// If not provided or null, defaults to the current UTC time.
    /// </summary>
    public DateTime? DetectedAt { get; } = DetectedAt ?? DateTime.UtcNow;

    /// <summary>
    /// Gets optional custom metadata associated with this face detection.
    /// If null is provided, an empty dictionary is used.
    /// </summary>
    public Dictionary<string, string> Metadata { get; } = Metadata ?? new Dictionary<string, string>();

    /// <summary>
    /// Determines whether the face was detected with high confidence.
    /// </summary>
    /// <returns>True if Confidence is greater than or equal to 0.8; otherwise, false</returns>
    public bool IsHighConfidence() => Confidence >= 0.8f;

    /// <summary>
    /// Determines whether this face has been successfully identified.
    /// </summary>
    /// <returns>True if RecognizedIdentity is not null; otherwise, false</returns>
    public bool IsIdentified() => RecognizedIdentity is not null;

    /// <summary>
    /// Determines whether eye state information is available for this face.
    /// </summary>
    /// <returns>True if EyesOpen has a value; otherwise, false</returns>
    public bool HasEyes() => EyesOpen.HasValue;

    /// <summary>
    /// Determines whether this face is detected as smiling.
    /// </summary>
    /// <returns>True if IsSmiling has a value and is true; otherwise, false</returns>
    public bool IsSmilingFace() => IsSmiling.HasValue && IsSmiling.Value;

    /// <summary>
    /// Calculates the aspect ratio of the detected face.
    /// </summary>
    /// <returns>The width-to-height ratio as a float (width / height)</returns>
    public float AspectRatio() => (float)FaceWidth / FaceHeight;

    /// <summary>
    /// Deserializes the bounding box JSON string into a rectangle with coordinates.
    /// </summary>
    /// <returns>A tuple containing (x, y, width, height) of the bounding box</returns>
    /// <exception cref="ArgumentException">Thrown when BoundingBox JSON is invalid or missing required properties</exception>
    public (float X, float Y, float Width, float Height) BoundingBoxAsRect()
    {
        try
        {
            using var doc = JsonDocument.Parse(BoundingBox);
            var root = doc.RootElement;

            if (!root.TryGetProperty("x", out var xProp) ||
                !root.TryGetProperty("y", out var yProp) ||
                !root.TryGetProperty("width", out var widthProp) ||
                !root.TryGetProperty("height", out var heightProp))
            {
                throw new ArgumentException(
                    "BoundingBox JSON must contain 'x', 'y', 'width', and 'height' numeric properties.",
                    nameof(BoundingBox));
            }

            return (
                xProp.GetSingle(),
                yProp.GetSingle(),
                widthProp.GetSingle(),
                heightProp.GetSingle()
            );
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "BoundingBox must be valid JSON with numeric x, y, width, height properties.",
                nameof(BoundingBox),
                ex);
        }
    }

    /// <summary>
    /// Validates that the FaceId is not null and matches valid UUID format.
    /// </summary>
    /// <param name="faceId">The FaceId to validate</param>
    /// <returns>The validated FaceId</returns>
    /// <exception cref="ArgumentNullException">Thrown when FaceId is null</exception>
    /// <exception cref="ArgumentException">Thrown when FaceId is not a valid UUID format</exception>
    private static string ValidateFaceId(string faceId)
    {
        Guard.NotEmpty(faceId, nameof(faceId));

        if (!Guid.TryParse(faceId, out _))
            throw new ArgumentException(
                "FaceId must be in valid UUID format: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",
                nameof(faceId));

        return faceId;
    }

    /// <summary>
    /// Validates that the Confidence score is between 0.0 and 1.0.
    /// </summary>
    /// <param name="confidence">The Confidence to validate</param>
    /// <returns>The validated Confidence</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when Confidence is outside the range [0.0, 1.0]</exception>
    private static float ValidateConfidence(float confidence)
    {
        if (confidence < 0.0f || confidence > 1.0f)
            throw new ArgumentOutOfRangeException(
                nameof(confidence),
                confidence,
                "Confidence must be between 0.0 and 1.0");

        return confidence;
    }

    /// <summary>
    /// Validates that the BoundingBox is a valid JSON string containing x, y, width, height properties.
    /// </summary>
    /// <param name="boundingBox">The BoundingBox to validate</param>
    /// <returns>The validated BoundingBox</returns>
    /// <exception cref="ArgumentNullException">Thrown when BoundingBox is null or empty</exception>
    /// <exception cref="ArgumentException">Thrown when BoundingBox is not valid JSON or missing required properties</exception>
    private static string ValidateBoundingBox(string boundingBox)
    {
        Guard.NotEmpty(boundingBox, nameof(boundingBox));

        try
        {
            using var doc = JsonDocument.Parse(boundingBox);
            var root = doc.RootElement;

            var hasX = root.TryGetProperty("x", out var xProp) && (xProp.ValueKind == JsonValueKind.Number);
            var hasY = root.TryGetProperty("y", out var yProp) && (yProp.ValueKind == JsonValueKind.Number);
            var hasWidth = root.TryGetProperty("width", out var widthProp) && (widthProp.ValueKind == JsonValueKind.Number);
            var hasHeight = root.TryGetProperty("height", out var heightProp) && (heightProp.ValueKind == JsonValueKind.Number);

            if (!hasX || !hasY || !hasWidth || !hasHeight)
                throw new ArgumentException(
                    "BoundingBox JSON must contain numeric 'x', 'y', 'width', and 'height' properties.",
                    nameof(boundingBox));

            return boundingBox;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException(
                "BoundingBox must be valid JSON with numeric x, y, width, height properties.",
                nameof(boundingBox),
                ex);
        }
    }

    /// <summary>
    /// Validates that the LandmarkPoints dictionary contains at least 5 entries with valid coordinates.
    /// </summary>
    /// <param name="landmarkPoints">The LandmarkPoints dictionary to validate</param>
    /// <returns>The validated LandmarkPoints</returns>
    /// <exception cref="ArgumentNullException">Thrown when LandmarkPoints is null</exception>
    /// <exception cref="ArgumentException">Thrown when LandmarkPoints has fewer than 5 entries</exception>
    private static Dictionary<string, (float X, float Y)> ValidateLandmarkPoints(Dictionary<string, (float X, float Y)> landmarkPoints)
    {
        Guard.NotNull(landmarkPoints, nameof(landmarkPoints));

        if (landmarkPoints.Count < 5)
            throw new ArgumentException(
                "LandmarkPoints must contain at least 5 facial landmark entries (e.g., left_eye, right_eye, nose, left_mouth, right_mouth).",
                nameof(landmarkPoints));

        return landmarkPoints;
    }

    /// <summary>
    /// Validates that the Orientation is within the range [-180, 180] degrees.
    /// </summary>
    /// <param name="orientation">The Orientation to validate</param>
    /// <returns>The validated Orientation</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when Orientation is outside [-180, 180]</exception>
    private static float ValidateOrientation(float orientation)
    {
        if (orientation < -180f || orientation > 180f)
            throw new ArgumentOutOfRangeException(
                nameof(orientation),
                orientation,
                "Orientation must be between -180 and 180 degrees");

        return orientation;
    }

    /// <summary>
    /// Validates that the FaceWidth is at least 10 pixels.
    /// </summary>
    /// <param name="faceWidth">The FaceWidth to validate</param>
    /// <returns>The validated FaceWidth</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when FaceWidth is less than 10</exception>
    private static int ValidateFaceWidth(int faceWidth)
    {
        if (faceWidth < 10)
            throw new ArgumentOutOfRangeException(
                nameof(faceWidth),
                faceWidth,
                "FaceWidth must be at least 10 pixels");

        return faceWidth;
    }

    /// <summary>
    /// Validates that the FaceHeight is at least 10 pixels.
    /// </summary>
    /// <param name="faceHeight">The FaceHeight to validate</param>
    /// <returns>The validated FaceHeight</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when FaceHeight is less than 10</exception>
    private static int ValidateFaceHeight(int faceHeight)
    {
        if (faceHeight < 10)
            throw new ArgumentOutOfRangeException(
                nameof(faceHeight),
                faceHeight,
                "FaceHeight must be at least 10 pixels");

        return faceHeight;
    }

    /// <summary>
    /// Validates that the IdentityConfidence is between 0.0 and 1.0, if provided.
    /// </summary>
    /// <param name="identityConfidence">The IdentityConfidence to validate</param>
    /// <returns>The validated IdentityConfidence</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when IdentityConfidence is outside [0.0, 1.0]</exception>
    private static float? ValidateIdentityConfidence(float? identityConfidence)
    {
        if (identityConfidence.HasValue && (identityConfidence.Value < 0.0f || identityConfidence.Value > 1.0f))
            throw new ArgumentOutOfRangeException(
                nameof(identityConfidence),
                identityConfidence,
                "IdentityConfidence must be between 0.0 and 1.0");

        return identityConfidence;
    }
}
