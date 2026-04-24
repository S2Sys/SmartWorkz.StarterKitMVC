using MassTransit;
using Microsoft.Extensions.Logging;
using SmartWorkz.StarterKitMVC.Application.Repositories;

namespace SmartWorkz.StarterKitMVC.Application.Events.Consumers;

/// <summary>
/// Consumer for handling email sent events.
/// Logs email delivery and updates email queue status.
/// </summary>
public class EmailSentEventConsumer : IConsumer<EmailSentEvent>
{
    private readonly IEmailQueueRepository _emailQueueRepository;
    private readonly ILogger<EmailSentEventConsumer> _logger;

    public EmailSentEventConsumer(
        IEmailQueueRepository emailQueueRepository,
        ILogger<EmailSentEventConsumer> logger)
    {
        _emailQueueRepository = emailQueueRepository ?? throw new ArgumentNullException(nameof(emailQueueRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<EmailSentEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation(
                "Processing email sent event - To: {To}, Subject: {Subject}, MessageId: {MessageId}",
                @event.To,
                @event.Subject,
                @event.MessageId);

            // Update email queue status to sent
            // TODO: Implement email queue status update when repository method is available
            // await _emailQueueRepository.UpdateEmailStatusAsync(@event.MessageId, "Sent");

            _logger.LogInformation(
                "Email sent event processed successfully - MessageId: {MessageId}",
                @event.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing email sent event - MessageId: {MessageId}",
                @event.MessageId);
            throw;
        }
    }
}
