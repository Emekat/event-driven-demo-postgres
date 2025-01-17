using api.Contracts;

namespace api.Services;

public class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public Task NotifyUser(string title, string message, string? userId = null)
    {
        logger.LogInformation("User notified");
        return Task.CompletedTask;
    }
}