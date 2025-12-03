namespace SmartWorkz.StarterKitMVC.Application.Notifications;

public interface INotificationTemplate
{
    string TemplateKey { get; }
    string Render(IDictionary<string, object> model);
}
