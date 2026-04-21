namespace SmartWorkz.Core.Shared.FileStorage;

using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartWorkz.Core.Services.FileStorage;

public static class FileStorageStartupExtensions
{
    public static IServiceCollection AddLocalFileStorage(this IServiceCollection services, string baseDirectory)
    {
        services.AddScoped<IFileStorageService>(provider =>
            new LocalFileStorageService(baseDirectory, provider.GetRequiredService<ILogger<LocalFileStorageService>>())
        );
        return services;
    }

    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, string connectionString, string containerName)
    {
        services.AddScoped<IFileStorageService>(provider =>
        {
            var client = new BlobContainerClient(new Uri($"https://{connectionString}/{containerName}"), new Azure.Storage.StorageSharedKeyCredential(connectionString, "key"));
            return new AzureBlobStorageService(client, provider.GetRequiredService<ILogger<AzureBlobStorageService>>());
        });
        return services;
    }
}
