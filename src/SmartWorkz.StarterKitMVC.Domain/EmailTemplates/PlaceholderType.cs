namespace SmartWorkz.StarterKitMVC.Domain.EmailTemplates;

/// <summary>
/// Defines the data type of a template placeholder.
/// </summary>
public enum PlaceholderType
{
    /// <summary>Plain text value.</summary>
    Text,
    
    /// <summary>HTML content.</summary>
    Html,
    
    /// <summary>Date value (formatted).</summary>
    Date,
    
    /// <summary>URL/link value.</summary>
    Url,
    
    /// <summary>Image URL.</summary>
    Image,
    
    /// <summary>Numeric value.</summary>
    Number,
    
    /// <summary>Currency value.</summary>
    Currency,
    
    /// <summary>Boolean value.</summary>
    Boolean,
    
    /// <summary>List/collection of items.</summary>
    List
}
