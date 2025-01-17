namespace api.Contracts;

public interface INotificationService
{
    Task NotifyUser(string title, string message, string? userId = null);
}