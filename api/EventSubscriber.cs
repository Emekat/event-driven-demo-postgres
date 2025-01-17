using api.Contracts;
using Play.Messaging.Events;
using Play.Messaging.Models;
using Play.PostgresEventStore;

namespace api;

public class EventSubscriber : BackgroundService
{
    private readonly IEventStore _eventStore;
    private readonly ILogger<EventSubscriber> _logger;
    private readonly IServiceProvider _serviceProvider;


    public EventSubscriber(IEventStore eventStore, Logger<EventSubscriber> logger,
        IServiceProvider serviceProvider)
    {
        _eventStore = eventStore;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       var lastProcessedEventId = await ((PostgresEventStore)_eventStore).GetLatestSequenceNumber();
        _logger.LogInformation("Starting from sequence number: {SequenceNumber}", lastProcessedEventId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Poll for new events
                var events = await _eventStore.GetEventsAfterAsync(lastProcessedEventId, 100);

                var eventList = events.ToList();
                if(!eventList.Any())
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }
                
                foreach (var @event in eventList)
                {
                    await ProcessEvent(@event);
                    lastProcessedEventId = @event.SequenceNumber; 
                    
                    await _eventStore.UpdateEventStatusAsync(lastProcessedEventId, true);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing events");
                await Task.Delay(5000, stoppingToken); // Back off on error
            }
        }
        _logger.LogInformation("Event Processing Service stopping...");
    }
    
    private async Task ProcessEvent(IEvent @event)
    {
        using var scope = _serviceProvider.CreateScope();
        
        try
        {
            switch (@event)
            {
                case UserCreatedEvent userCreated:
                    var notificationProcessor = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationProcessor.NotifyUser("Welcome", "Welcome to our platform!", userCreated.UserId);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventId}", @event.Id);
        }
    }
}
