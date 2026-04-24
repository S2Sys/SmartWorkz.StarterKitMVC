namespace SmartWorkz.StarterKitMVC.Application.Events;

/// <summary>
/// Event published after an email is successfully sent.
/// </summary>
public class EmailSentEvent
{
    public EmailSentEvent(string to, string subject, string messageId)
    {
        To = to;
        Subject = subject;
        MessageId = messageId;
        SentAt = DateTime.UtcNow;
    }

    public string To { get; }
    public string Subject { get; }
    public string MessageId { get; }
    public DateTime SentAt { get; }
}
