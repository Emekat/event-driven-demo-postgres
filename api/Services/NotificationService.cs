using api.Contracts;

namespace api.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }
    public Task NotifyUser(string title, string message, string? userId = null)
    {
        _logger.LogInformation("User notified");
        return Task.CompletedTask;
    }
}