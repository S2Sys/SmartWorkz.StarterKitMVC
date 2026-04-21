namespace SmartWorkz.Shared;

using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class FileStorageStartupExtensions
{
    /// <summary>
    /// Registers local file storage service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="baseDirectory">The base directory for file storage.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if baseDirectory is null or empty.</exception>
    public static IServiceCollection AddLocalFileStorage(this IServiceCollection services, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
            throw new ArgumentException("Base directory cannot be null or empty", nameof(baseDirectory));

        services.AddScoped<IFileStorageService>(provider =>
            new LocalFileStorageService(baseDirectory, provider.GetRequiredService<ILogger<LocalFileStorageService>>())
        );
        return services;
    }

    /// <summary>
    /// Registers Azure Blob Storage service using connection string.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The Azure Storage connection string (e.g., DefaultEndpointsProtocol=https;AccountName=...)</param>
    /// <param name="containerName">The blob container name.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if connectionString or containerName is null or empty.</exception>
    /// <remarks>
    /// The connection string should follow the format:
    /// DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net
    /// </remarks>
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, string connectionString, string containerName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name cannot be null or empty", nameof(containerName));

        services.AddScoped<IFileStorageService>(provider =>
        {
            // Use the connection string directly - BlobContainerClient will parse it
            var containerClient = new BlobContainerClient(connectionString, containerName);
            return new AzureBlobStorageService(containerClient, provider.GetRequiredService<ILogger<AzureBlobStorageService>>());
        });
        return services;
    }
}
