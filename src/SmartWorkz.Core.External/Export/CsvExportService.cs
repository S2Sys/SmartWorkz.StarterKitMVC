namespace SmartWorkz.Core.External.Export;

using Microsoft.Extensions.Logging;

/// <summary>
/// Sealed implementation of IExportService for exporting data to CSV format using CsvHelper.
/// Provides generic collection export with automatic property discovery via reflection,
/// special character escaping, and configurable export options.
/// </summary>
public sealed class CsvExportService : IExportService
{
    private readonly ILogger<CsvExportService>? _logger;

    /// <summary>
    /// Initializes a new instance of the CsvExportService class.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    public CsvExportService(ILogger<CsvExportService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Exports an enumerable collection to CSV format as a byte array.
    /// Performs automatic property discovery via reflection to generate headers and rows.
    /// Handles special characters with proper escaping (quotes, commas, newlines).
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="data">The data to export.</param>
    /// <param name="options">Optional configuration for the export. Uses defaults if null.</param>
    /// <param name="cancellationToken">Cancellation token for async operation cancellation.</param>
    /// <returns>A Result containing the CSV bytes if successful; failure if data is empty or error occurs.</returns>
    public async Task<Result<byte[]>> ExportAsync<T>(
        IEnumerable<T> data,
        ExportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Use default options if none provided
            options ??= new ExportOptions();

            // Convert to list to allow multiple enumerations
            var dataList = data.ToList();

            // Validate that data is not empty
            if (dataList.Count == 0)
            {
                _logger?.LogWarning("CSV export attempted with empty data collection");
                return Result<byte[]>.Fail<byte[]>(
                    "Error.EmptyDataCollection",
                    "Cannot export an empty data collection to CSV.");
            }

            // Get the type information for property discovery
            var itemType = typeof(T);
            var properties = itemType.GetProperties(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.IgnoreCase);

            if (properties.Length == 0)
            {
                _logger?.LogWarning($"No public properties found on type {itemType.Name}");
                return Result<byte[]>.Fail<byte[]>(
                    "Error.NoPropertiesFound",
                    $"Type {itemType.Name} has no public properties to export.");
            }

            // Generate CSV content
            var csvBytes = await GenerateCsvBytesAsync(dataList, properties, options, cancellationToken);

            _logger?.LogInformation($"Successfully exported {dataList.Count} items to CSV");
            return Result<byte[]>.Ok(csvBytes);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("CSV export was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error exporting CSV");
            return Result<byte[]>.Fail<byte[]>(
                "Error.CsvExportFailed",
                $"CSV export failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates CSV bytes from the provided data and properties.
    /// </summary>
    private async Task<byte[]> GenerateCsvBytesAsync<T>(
        List<T> data,
        System.Reflection.PropertyInfo[] properties,
        ExportOptions options,
        CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream, options.Encoding))
            using (var csvWriter = new CsvWriter(writer, GetCsvConfiguration(options)))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Write headers if requested
                if (options.IncludeHeaders)
                {
                    foreach (var property in properties)
                    {
                        csvWriter.WriteField(property.Name);
                    }
                    csvWriter.NextRecord();
                }

                // Write data rows
                foreach (var item in data)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var property in properties)
                    {
                        var value = property.GetValue(item);
                        csvWriter.WriteField(value);
                    }
                    csvWriter.NextRecord();
                }

                writer.Flush();
                return memoryStream.ToArray();
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Creates a CsvConfiguration object based on the export options.
    /// Configures delimiter, quoting strategy, and culture settings.
    /// </summary>
    private CsvConfiguration GetCsvConfiguration(ExportOptions options)
    {
        var config = new CsvConfiguration(
            options.UseCultureInfo
                ? System.Globalization.CultureInfo.CurrentCulture
                : System.Globalization.CultureInfo.InvariantCulture)
        {
            Delimiter = options.Delimiter.ToString(),
            // RFC 4180 compliant: always quote fields to handle special characters
            Quote = '"',
            HasHeaderRecord = options.IncludeHeaders,
        };

        return config;
    }
}
