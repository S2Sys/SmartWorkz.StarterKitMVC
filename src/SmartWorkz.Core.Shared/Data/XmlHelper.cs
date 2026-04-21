namespace SmartWorkz.Shared;

using System.Collections;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

/// <summary>
/// Provides static methods for XML serialization, deserialization, and XPath queries.
/// Uses System.Xml.Linq for manipulation and reflection for property mapping.
/// Sealed class to prevent inheritance and ensure consistent behavior.
/// </summary>
public sealed class XmlHelper
{
    private XmlHelper()
    {
        // Prevent instantiation
    }

    /// <summary>
    /// Serializes an object to an XML string using reflection.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="options">XML options. If null, defaults are used.</param>
    /// <returns>A Result containing the XML string if successful; otherwise a failure.</returns>
    public static Result<string> Serialize<T>(T? obj, XmlOptions? options = null) where T : class
    {
        try
        {
            if (obj == null)
                return Result.Fail<string>("XML.SERIALIZE.NULL_OBJECT", "Object cannot be null.");

            options ??= new XmlOptions();

            var rootElement = new XElement(options.RootElement);
            SerializeObject(obj, rootElement);

            var xmlDoc = new XDocument(rootElement);
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = options.Indent,
                IndentChars = options.Indent ? "  " : "",
                OmitXmlDeclaration = !options.IncludeXmlDeclaration,
                ConformanceLevel = ConformanceLevel.Document
            };

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                xmlDoc.WriteTo(writer);
            }

            return Result.Ok(sb.ToString());
        }
        catch (Exception ex)
        {
            return Result.Fail<string>(
                Error.FromException(ex, "XML.SERIALIZE.FAILED"));
        }
    }

    /// <summary>
    /// Deserializes an XML string to an object of type T using reflection.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <param name="options">XML options. If null, defaults are used.</param>
    /// <returns>A Result containing the deserialized object if successful; otherwise a failure.</returns>
    public static Result<T> Deserialize<T>(string xml, XmlOptions? options = null) where T : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xml))
                return Result.Fail<T>("XML.DESERIALIZE.EMPTY", "XML string cannot be null or empty.");

            options ??= new XmlOptions();

            var xmlDoc = XDocument.Parse(xml);
            var rootElement = xmlDoc.Root;

            if (rootElement == null)
                return Result.Fail<T>("XML.DESERIALIZE.NO_ROOT", "XML document has no root element.");

            var instance = Activator.CreateInstance<T>();
            DeserializeObject(rootElement, instance);

            return Result.Ok(instance);
        }
        catch (Exception ex)
        {
            return Result.Fail<T>(
                Error.FromException(ex, "XML.DESERIALIZE.FAILED"));
        }
    }

    /// <summary>
    /// Executes an XPath query on an XML string and returns matching element values.
    /// </summary>
    /// <param name="xml">The XML string to query.</param>
    /// <param name="xpathExpression">The XPath expression to execute.</param>
    /// <returns>A Result containing a list of matched values if successful; otherwise a failure.</returns>
    public static Result<List<string>> Query(string xml, string xpathExpression)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(xml))
                return Result.Fail<List<string>>("XML.QUERY.EMPTY", "XML string cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(xpathExpression))
                return Result.Fail<List<string>>("XML.QUERY.EMPTY_EXPRESSION", "XPath expression cannot be null or empty.");

            var xmlDoc = XDocument.Parse(xml);
            var results = new List<string>();

            // Handle different XPath patterns
            var elements = xmlDoc.XPathSelectElements(xpathExpression);
            foreach (var element in elements)
            {
                // If element has child text, get the text value
                if (element.HasElements)
                {
                    // If element has only one text node, return it; otherwise return element name
                    var textNode = element.Nodes().OfType<XText>().FirstOrDefault();
                    if (textNode != null)
                    {
                        results.Add(textNode.Value);
                    }
                    else
                    {
                        results.Add(element.Name.LocalName);
                    }
                }
                else
                {
                    results.Add(element.Value);
                }
            }

            return Result.Ok(results);
        }
        catch (Exception ex)
        {
            return Result.Fail<List<string>>(
                Error.FromException(ex, "XML.QUERY.FAILED"));
        }
    }

    /// <summary>
    /// Recursively serializes an object's properties into an XML element.
    /// </summary>
    private static void SerializeObject(object obj, XElement parent)
    {
        if (obj == null)
            return;

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip non-readable properties
            if (!property.CanRead)
                continue;

            var value = property.GetValue(obj);

            // Handle null values
            if (value == null)
                continue;

            var propertyName = XmlConvert.EncodeLocalName(property.Name);

            // Handle basic types
            if (IsBasicType(property.PropertyType))
            {
                var element = new XElement(propertyName, ConvertToXmlValue(value));
                parent.Add(element);
            }
            // Handle DateTime
            else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            {
                if (value is DateTime dt)
                {
                    var element = new XElement(propertyName, dt.ToString("o")); // ISO 8601 format
                    parent.Add(element);
                }
            }
            // Handle List<T>
            else if (IsGenericList(property.PropertyType))
            {
                var list = value as IEnumerable;
                if (list != null)
                {
                    var listElement = new XElement(propertyName);
                    var itemType = property.PropertyType.GetGenericArguments()[0];

                    foreach (var item in list)
                    {
                        if (IsBasicType(itemType))
                        {
                            var itemElement = new XElement("Item", ConvertToXmlValue(item));
                            listElement.Add(itemElement);
                        }
                        else
                        {
                            var itemElement = new XElement("Item");
                            SerializeObject(item, itemElement);
                            listElement.Add(itemElement);
                        }
                    }

                    parent.Add(listElement);
                }
            }
            // Handle nested objects
            else if (IsComplexType(property.PropertyType))
            {
                var objectElement = new XElement(propertyName);
                SerializeObject(value, objectElement);
                parent.Add(objectElement);
            }
        }
    }

    /// <summary>
    /// Recursively deserializes an XML element into an object's properties.
    /// </summary>
    private static void DeserializeObject(XElement element, object target)
    {
        if (element == null || target == null)
            return;

        var type = target.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip non-writable properties
            if (!property.CanWrite)
                continue;

            var propertyName = XmlConvert.EncodeLocalName(property.Name);
            var childElement = element.Element(propertyName);

            if (childElement == null)
                continue;

            try
            {
                // Handle basic types
                if (IsBasicType(property.PropertyType))
                {
                    var value = ConvertFromXmlValue(childElement.Value, property.PropertyType);
                    property.SetValue(target, value);
                }
                // Handle DateTime
                else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                {
                    if (DateTime.TryParseExact(childElement.Value, "o", null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                    {
                        if (property.PropertyType == typeof(DateTime?))
                        {
                            property.SetValue(target, new DateTime?(dt));
                        }
                        else
                        {
                            property.SetValue(target, dt);
                        }
                    }
                }
                // Handle List<T>
                else if (IsGenericList(property.PropertyType))
                {
                    var itemType = property.PropertyType.GetGenericArguments()[0];
                    var listType = typeof(List<>).MakeGenericType(itemType);
                    var list = Activator.CreateInstance(listType) as IList;

                    if (list != null)
                    {
                        var itemElements = childElement.Elements("Item");
                        foreach (var itemElement in itemElements)
                        {
                            if (IsBasicType(itemType))
                            {
                                var value = ConvertFromXmlValue(itemElement.Value, itemType);
                                list.Add(value);
                            }
                            else
                            {
                                var item = Activator.CreateInstance(itemType);
                                DeserializeObject(itemElement, item);
                                list.Add(item);
                            }
                        }

                        property.SetValue(target, list);
                    }
                }
                // Handle nested objects
                else if (IsComplexType(property.PropertyType))
                {
                    var nestedObject = Activator.CreateInstance(property.PropertyType);
                    if (nestedObject != null)
                    {
                        DeserializeObject(childElement, nestedObject);
                        property.SetValue(target, nestedObject);
                    }
                }
            }
            catch
            {
                // Skip properties that cannot be deserialized
            }
        }
    }

    /// <summary>
    /// Determines if a type is a basic/primitive type supported by XML.
    /// </summary>
    private static bool IsBasicType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType == typeof(string)
            || underlyingType == typeof(int)
            || underlyingType == typeof(long)
            || underlyingType == typeof(short)
            || underlyingType == typeof(byte)
            || underlyingType == typeof(decimal)
            || underlyingType == typeof(double)
            || underlyingType == typeof(float)
            || underlyingType == typeof(bool)
            || underlyingType == typeof(Guid)
            || underlyingType.IsEnum;
    }

    /// <summary>
    /// Determines if a type is a generic List&lt;T&gt;.
    /// </summary>
    private static bool IsGenericList(Type type)
    {
        return type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(List<>);
    }

    /// <summary>
    /// Determines if a type is a complex (non-primitive) type.
    /// </summary>
    private static bool IsComplexType(Type type)
    {
        return !IsBasicType(type)
            && type != typeof(DateTime)
            && type != typeof(DateTime?)
            && !IsGenericList(type)
            && type != typeof(string);
    }

    /// <summary>
    /// Converts a value to its XML-safe string representation.
    /// </summary>
    private static string ConvertToXmlValue(object? value)
    {
        if (value == null)
            return string.Empty;

        if (value is bool boolValue)
            return boolValue.ToString().ToLowerInvariant();

        if (value is DateTime dtValue)
            return dtValue.ToString("o");

        return value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Converts an XML string value to the specified type.
    /// </summary>
    private static object? ConvertFromXmlValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlyingType == typeof(string))
                return value;

            if (underlyingType == typeof(int))
                return int.Parse(value);

            if (underlyingType == typeof(long))
                return long.Parse(value);

            if (underlyingType == typeof(short))
                return short.Parse(value);

            if (underlyingType == typeof(byte))
                return byte.Parse(value);

            if (underlyingType == typeof(decimal))
                return decimal.Parse(value);

            if (underlyingType == typeof(double))
                return double.Parse(value);

            if (underlyingType == typeof(float))
                return float.Parse(value);

            if (underlyingType == typeof(bool))
                return bool.Parse(value);

            if (underlyingType == typeof(Guid))
                return Guid.Parse(value);

            if (underlyingType == typeof(DateTime))
                return DateTime.ParseExact(value, "o", null, System.Globalization.DateTimeStyles.RoundtripKind);

            if (underlyingType.IsEnum)
                return Enum.Parse(underlyingType, value);

            return value;
        }
        catch
        {
            return null;
        }
    }
}
