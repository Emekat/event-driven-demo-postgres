using Play.Messaging.Models;

namespace Play.EventDispatcher;

public interface IEventDispatcher
{
    Task DispatchAsync(IEvent @event);
    void Subscribe<T>(Func<T, Task> handler) where T : IEvent;
}

public class EventDispatcher : IEventDispatcher
{
    private readonly Dictionary<Type, List<Func<IEvent, Task>>> _handlers = new();

    public void Subscribe<T>(Func<T, Task> handler) where T : IEvent
    {
        var eventType = typeof(T);
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Func<IEvent, Task>>();

        _handlers[eventType].Add((@event) => handler((T)@event));
    }

    public async Task DispatchAsync(IEvent @event)
    {
        var eventType = @event.GetType();
        if (_handlers.ContainsKey(eventType))
        {
            foreach (var handler in _handlers[eventType])
            {
                await handler(@event);
            }
        }
    }
}
