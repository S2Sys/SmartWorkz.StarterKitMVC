namespace SmartWorkz.Sample.ECommerce.Web.Models;

/// <summary>
/// Flat DTO for exporting order data to CSV, Excel, and PDF formats.
/// Uses primitive properties only (no nested objects) for clean serialization.
/// </summary>
public record OrderExportDto(
    int Id,
    int CustomerId,
    string Status,
    decimal Total,
    string Currency,
    DateTime PlacedAt,
    int ItemCount
);
