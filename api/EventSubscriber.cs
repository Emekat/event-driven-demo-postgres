using api.Contracts;
using Play.Messaging.Events;
using Play.Messaging.Models;
using Play.PostgresEventStore;

namespace api;

public class EventSubscriber(ILogger<EventSubscriber> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting processing");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
                var lastProcessedEventId = await eventStore.GetLatestSequenceNumber();
                var events = await eventStore.GetEventsAfterAsync(lastProcessedEventId, 100);

                var eventList = events.ToList();
                if(!eventList.Any())
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }
                
                foreach (var @event in eventList)
                {
                    await ProcessEvent(@event);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing events");
                await Task.Delay(5000, stoppingToken); // Back off on error
            }
        }
        logger.LogInformation("Event Processing Service stopping...");
    }
    
    private async Task ProcessEvent(IEvent @event)
    {
        using var scope = serviceProvider.CreateScope();
        
        try
        {
            switch (@event)
            {
                case UserRegisteredEvent userCreated:
                    var notificationProcessor = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationProcessor.NotifyUser("Welcome", "Welcome to our platform!", userCreated.UserId);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing event {EventId}", @event.Id);
            await HandleEventProcessingError(@event, ex);
        }
    }
    
    private async Task HandleEventProcessingError(IEvent @event, Exception error)
    {
        using var scope = serviceProvider.CreateScope();
        var failedEventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        await failedEventStore.SaveFailedEventAsync(@event, error);

        // Optionally notify administrators or trigger alerts
        // await _notificationService.NotifyAdministrators(failedEvent);
    }
}
