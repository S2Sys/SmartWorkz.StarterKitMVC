using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.EmailTemplates;
using SmartWorkz.StarterKitMVC.Application.Notifications;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;
using SmartWorkz.StarterKitMVC.Shared.Models;

namespace SmartWorkz.StarterKitMVC.Infrastructure.EmailTemplates;

/// <summary>
/// Default implementation of templated email sender.
/// Integrates email templates with the notification queue and persists emails to the database.
/// </summary>
public sealed class TemplatedEmailSender : ITemplatedEmailSender
{
    private readonly IEmailTemplateService _templateService;
    private readonly INotificationQueue _notificationQueue;
    private readonly IEmailQueueRepository _emailQueueRepository;
    private readonly ILogger<TemplatedEmailSender> _logger;

    public TemplatedEmailSender(
        IEmailTemplateService templateService,
        INotificationQueue notificationQueue,
        IEmailQueueRepository emailQueueRepository,
        ILogger<TemplatedEmailSender> logger)
    {
        _templateService = templateService;
        _notificationQueue = notificationQueue;
        _emailQueueRepository = emailQueueRepository;
        _logger = logger;
    }

    public async Task<bool> SendTemplatedEmailAsync(
        string templateId,
        string recipient,
        IDictionary<string, object> data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Render the template
            var result = await _templateService.RenderTemplateAsync(templateId, data, cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning("Failed to render template {TemplateId}: {Errors}",
                    templateId, string.Join(", ", result.Errors));
                return false;
            }

            // Create notification message for in-memory queue (real-time notifications)
            var message = new NotificationMessage(
                Channel: NotificationChannel.Email,
                Recipient: recipient,
                Subject: result.Subject,
                Body: result.HtmlBody,
                Metadata: new Dictionary<string, string>
                {
                    ["TemplateId"] = templateId,
                    ["PlainText"] = result.PlainTextBody ?? string.Empty
                }
            );

            // Queue the notification (in-memory)
            await _notificationQueue.EnqueueAsync(message, cancellationToken);

            // Also persist to database queue for reliable delivery & audit trail
            var emailQueue = new EmailQueue
            {
                ToEmail = recipient,
                Subject = result.Subject,
                Body = result.HtmlBody,
                IsHtml = true,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                SendAttempts = 0
            };

            await _emailQueueRepository.EnqueueAsync(emailQueue, cancellationToken);

            _logger.LogInformation("Queued templated email {TemplateId} to {Recipient}", templateId, recipient);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send templated email {TemplateId} to {Recipient}", templateId, recipient);
            return false;
        }
    }

    public async Task<int> SendTemplatedEmailAsync(
        string templateId,
        IEnumerable<string> recipients,
        IDictionary<string, object> data,
        CancellationToken cancellationToken = default)
    {
        var count = 0;
        
        foreach (var recipient in recipients)
        {
            if (await SendTemplatedEmailAsync(templateId, recipient, data, cancellationToken))
            {
                count++;
            }
        }
        
        return count;
    }

    public async Task<int> SendPersonalizedEmailsAsync(
        string templateId,
        IDictionary<string, IDictionary<string, object>> recipientData,
        CancellationToken cancellationToken = default)
    {
        var count = 0;
        
        foreach (var (recipient, data) in recipientData)
        {
            if (await SendTemplatedEmailAsync(templateId, recipient, data, cancellationToken))
            {
                count++;
            }
        }
        
        return count;
    }
}
