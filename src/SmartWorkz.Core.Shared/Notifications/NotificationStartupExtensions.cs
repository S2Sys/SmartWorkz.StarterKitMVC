namespace SmartWorkz.Core.Shared.Notifications;

using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.Core.Services.Notifications;
using System.IO;

public static class NotificationStartupExtensions
{
    public static IServiceCollection AddFirebaseCloudMessaging(
        this IServiceCollection services,
        string serviceAccountPath)
    {
        if (!System.IO.File.Exists(serviceAccountPath))
            throw new FileNotFoundException($"Firebase service account file not found: {serviceAccountPath}");

        var credential = GoogleCredential.FromFile(serviceAccountPath);
        FirebaseApp.Create(new AppOptions { Credential = credential });

        services.AddScoped<IPushNotificationService, FirebaseCloudMessagingService>();
        return services;
    }
}
