namespace SmartWorkz.Shared;

/// <summary>
/// Configuration options for XML serialization, deserialization, and query operations.
/// Sealed to prevent inheritance and ensure consistent behavior.
/// </summary>
public sealed class XmlOptions
{
    /// <summary>
    /// Gets or sets the name of the root element in the generated XML.
    /// Default is "Root".
    /// </summary>
    public string RootElement { get; set; } = "Root";

    /// <summary>
    /// Gets or sets a value indicating whether to include the XML declaration.
    /// Default is true.
    /// </summary>
    public bool IncludeXmlDeclaration { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to format the output XML with indentation.
    /// Default is true.
    /// </summary>
    public bool Indent { get; set; } = true;

    /// <summary>
    /// Creates a default instance of XmlOptions.
    /// </summary>
    public XmlOptions()
    {
    }

    /// <summary>
    /// Creates an instance of XmlOptions with a specified root element name.
    /// </summary>
    /// <param name="rootElement">The name of the root element.</param>
    public XmlOptions(string rootElement)
    {
        RootElement = rootElement;
    }

    /// <summary>
    /// Creates an instance of XmlOptions with specified configuration.
    /// </summary>
    /// <param name="rootElement">The name of the root element.</param>
    /// <param name="includeXmlDeclaration">Whether to include the XML declaration.</param>
    /// <param name="indent">Whether to indent the output.</param>
    public XmlOptions(string rootElement, bool includeXmlDeclaration, bool indent)
    {
        RootElement = rootElement;
        IncludeXmlDeclaration = includeXmlDeclaration;
        Indent = indent;
    }
}
