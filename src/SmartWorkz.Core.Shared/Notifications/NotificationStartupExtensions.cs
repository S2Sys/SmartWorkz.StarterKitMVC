namespace SmartWorkz.Shared;

using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Services.Notifications;
using System.IO;

public static class NotificationStartupExtensions
{
    /// <summary>
    /// Adds Firebase Cloud Messaging service to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceAccountPath">Path to the Firebase service account JSON file.</param>
    /// <returns>The modified service collection.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the service account file is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when Firebase credentials cannot be loaded.</exception>
    public static IServiceCollection AddFirebaseCloudMessaging(
        this IServiceCollection services,
        string serviceAccountPath)
    {
        if (!System.IO.File.Exists(serviceAccountPath))
            throw new FileNotFoundException($"Firebase service account file not found: {serviceAccountPath}");

        try
        {
            var credential = GoogleCredential.FromFile(serviceAccountPath);
            if (FirebaseApp.DefaultInstance == null)
                FirebaseApp.Create(new AppOptions { Credential = credential });
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to load Firebase credentials from {serviceAccountPath}", ex);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            // App already initialized, ignore
        }

        services.AddScoped<IPushNotificationService, FirebaseCloudMessagingService>();
        return services;
    }
}
