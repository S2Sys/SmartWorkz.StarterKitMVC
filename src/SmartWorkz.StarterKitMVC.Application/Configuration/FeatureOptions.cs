namespace SmartWorkz.StarterKitMVC.Application.Configuration;

/// <summary>
/// Root configuration for all plug-and-play features.
/// </summary>
public class FeatureOptions
{
    public const string SectionName = "Features";
    
    public IdentityFeatureOptions Identity { get; set; } = new();
    public AuthenticationFeatureOptions Authentication { get; set; } = new();
    public MultiTenancyFeatureOptions MultiTenancy { get; set; } = new();
    public CachingFeatureOptions Caching { get; set; } = new();
    public LoggingFeatureOptions Logging { get; set; } = new();
    public BackgroundJobsFeatureOptions BackgroundJobs { get; set; } = new();
    public EventBusFeatureOptions EventBus { get; set; } = new();
    public NotificationsFeatureOptions Notifications { get; set; } = new();
    public StorageFeatureOptions Storage { get; set; } = new();
    public AiFeatureOptions AI { get; set; } = new();
    public ApiVersioningFeatureOptions ApiVersioning { get; set; } = new();
    public RateLimitingFeatureOptions RateLimiting { get; set; } = new();
    public HealthChecksFeatureOptions HealthChecks { get; set; } = new();
    public SwaggerFeatureOptions Swagger { get; set; } = new();
    public LocalizationFeatureOptions Localization { get; set; } = new();
    public CompressionFeatureOptions Compression { get; set; } = new();
    public CorsFeatureOptions Cors { get; set; } = new();
    public SecurityFeatureOptions Security { get; set; } = new();
}

#region Identity & Authentication

public class IdentityFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "AspNetIdentity";
    public bool RequireConfirmedEmail { get; set; } = false;
    public PasswordPolicyOptions PasswordPolicy { get; set; } = new();
}

public class PasswordPolicyOptions
{
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public int MinimumLength { get; set; } = 8;
}

public class AuthenticationFeatureOptions
{
    public JwtOptions Jwt { get; set; } = new();
    public OAuthOptions OAuth { get; set; } = new();
    public TwoFactorOptions TwoFactor { get; set; } = new();
}

public class JwtOptions
{
    public bool Enabled { get; set; } = true;
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SmartWorkz";
    public string Audience { get; set; } = "StarterKitMVC";
    public int ExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

public class OAuthOptions
{
    public OAuthProviderOptions Google { get; set; } = new();
    public OAuthProviderOptions Microsoft { get; set; } = new() { TenantId = "common" };
    public OAuthProviderOptions GitHub { get; set; } = new();
    public OAuthProviderOptions Facebook { get; set; } = new();
}

public class OAuthProviderOptions
{
    public bool Enabled { get; set; } = false;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public string? AppId { get; set; }
    public string? AppSecret { get; set; }
}

public class TwoFactorOptions
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "Email";
    public AuthenticatorOptions Authenticator { get; set; } = new();
}

public class AuthenticatorOptions
{
    public bool Enabled { get; set; } = false;
    public string Issuer { get; set; } = "StarterKitMVC";
}

#endregion

#region Multi-Tenancy

public class MultiTenancyFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Strategy { get; set; } = "Subdomain"; // Subdomain, Header, Query, Cookie
    public string DefaultTenantId { get; set; } = "default";
    public string HeaderName { get; set; } = "X-Tenant-ID";
    public string QueryParamName { get; set; } = "tenantId";
}

#endregion

#region Caching

public class CachingFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Memory"; // Memory, Redis
    public int DefaultExpirationMinutes { get; set; } = 30;
    public RedisOptions Redis { get; set; } = new();
}

public class RedisOptions
{
    public bool Enabled { get; set; } = false;
    public string InstanceName { get; set; } = "StarterKit_";
}

#endregion

#region Logging

public class LoggingFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Serilog";
    public bool IncludeScopes { get; set; } = true;
    public SeqOptions Seq { get; set; } = new();
    public ApplicationInsightsOptions ApplicationInsights { get; set; } = new();
    public ElasticSearchOptions ElasticSearch { get; set; } = new();
}

public class SeqOptions
{
    public bool Enabled { get; set; } = false;
    public string ServerUrl { get; set; } = string.Empty;
}

public class ApplicationInsightsOptions
{
    public bool Enabled { get; set; } = false;
    public string ConnectionString { get; set; } = string.Empty;
}

public class ElasticSearchOptions
{
    public bool Enabled { get; set; } = false;
    public string NodeUri { get; set; } = string.Empty;
}

#endregion

#region Background Jobs

public class BackgroundJobsFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "InMemory"; // InMemory, Hangfire, Quartz
    public HangfireOptions Hangfire { get; set; } = new();
    public QuartzOptions Quartz { get; set; } = new();
}

public class HangfireOptions
{
    public bool Enabled { get; set; } = false;
    public string DashboardPath { get; set; } = "/hangfire";
    public bool DashboardAuthorization { get; set; } = true;
}

public class QuartzOptions
{
    public bool Enabled { get; set; } = false;
}

#endregion

#region Event Bus

public class EventBusFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "InMemory"; // InMemory, RabbitMQ, AzureServiceBus, Kafka
    public int RetryCount { get; set; } = 3;
    public RabbitMQOptions RabbitMQ { get; set; } = new();
    public AzureServiceBusOptions AzureServiceBus { get; set; } = new();
    public KafkaOptions Kafka { get; set; } = new();
}

public class RabbitMQOptions
{
    public bool Enabled { get; set; } = false;
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}

public class AzureServiceBusOptions
{
    public bool Enabled { get; set; } = false;
    public string ConnectionString { get; set; } = string.Empty;
}

public class KafkaOptions
{
    public bool Enabled { get; set; } = false;
    public string BootstrapServers { get; set; } = "localhost:9092";
}

#endregion

#region Notifications

public class NotificationsFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public EmailNotificationOptions Email { get; set; } = new();
    public SmsNotificationOptions Sms { get; set; } = new();
    public PushNotificationOptions Push { get; set; } = new();
    public SignalROptions SignalR { get; set; } = new();
}

public class EmailNotificationOptions
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Smtp";
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public SmtpOptions Smtp { get; set; } = new();
    public SendGridOptions SendGrid { get; set; } = new();
}

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SendGridOptions
{
    public bool Enabled { get; set; } = false;
    public string ApiKey { get; set; } = string.Empty;
}

public class SmsNotificationOptions
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "Twilio";
    public TwilioOptions Twilio { get; set; } = new();
}

public class TwilioOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}

public class PushNotificationOptions
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "Firebase";
    public FirebaseOptions Firebase { get; set; } = new();
    public OneSignalOptions OneSignal { get; set; } = new();
}

public class FirebaseOptions
{
    public string ServerKey { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
}

public class OneSignalOptions
{
    public bool Enabled { get; set; } = false;
    public string AppId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class SignalROptions
{
    public bool Enabled { get; set; } = true;
    public string HubPath { get; set; } = "/notificationHub";
}

#endregion

#region Storage

public class StorageFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Local"; // Local, Azure, S3
    public string BasePath { get; set; } = "uploads";
    public int MaxFileSizeMB { get; set; } = 10;
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".pdf" };
    public AzureBlobOptions Azure { get; set; } = new();
    public S3Options S3 { get; set; } = new();
}

public class AzureBlobOptions
{
    public bool Enabled { get; set; } = false;
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "uploads";
}

public class S3Options
{
    public bool Enabled { get; set; } = false;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
}

#endregion

#region AI

public class AiFeatureOptions
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "OpenAI";
    public OpenAIOptions OpenAI { get; set; } = new();
    public AzureOpenAIOptions AzureOpenAI { get; set; } = new();
}

public class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4";
    public int MaxTokens { get; set; } = 2000;
}

public class AzureOpenAIOptions
{
    public bool Enabled { get; set; } = false;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
}

#endregion

#region API & Infrastructure

public class ApiVersioningFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string DefaultVersion { get; set; } = "1.0";
    public bool ReportApiVersions { get; set; } = true;
    public bool AssumeDefaultVersionWhenUnspecified { get; set; } = true;
}

public class RateLimitingFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 10;
}

public class HealthChecksFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "/health";
    public HealthChecksUIOptions UI { get; set; } = new();
}

public class HealthChecksUIOptions
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "/health-ui";
}

public class SwaggerFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string Title { get; set; } = "StarterKitMVC API";
    public string Version { get; set; } = "v1";
    public string Description { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}

public class LocalizationFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string DefaultCulture { get; set; } = "en-US";
    public string[] SupportedCultures { get; set; } = { "en-US" };
}

public class CompressionFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public bool EnableForHttps { get; set; } = true;
}

public class CorsFeatureOptions
{
    public bool Enabled { get; set; } = true;
    public string PolicyName { get; set; } = "DefaultPolicy";
    public string[] AllowedOrigins { get; set; } = { "*" };
    public string[] AllowedMethods { get; set; } = { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
    public string[] AllowedHeaders { get; set; } = { "*" };
    public bool AllowCredentials { get; set; } = false;
}

public class SecurityFeatureOptions
{
    public HttpsOptions Https { get; set; } = new();
    public AntiforgeryOptions Antiforgery { get; set; } = new();
    public ContentSecurityPolicyOptions ContentSecurityPolicy { get; set; } = new();
}

public class HttpsOptions
{
    public bool Enabled { get; set; } = true;
    public bool RedirectHttps { get; set; } = true;
    public bool UseHsts { get; set; } = true;
}

public class AntiforgeryOptions
{
    public bool Enabled { get; set; } = true;
    public string HeaderName { get; set; } = "X-XSRF-TOKEN";
}

public class ContentSecurityPolicyOptions
{
    public bool Enabled { get; set; } = false;
}

#endregion
