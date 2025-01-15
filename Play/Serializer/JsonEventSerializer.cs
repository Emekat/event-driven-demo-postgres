using System.Text.Json;
using Play.Messaging.Models;

namespace Play.Serializer;
public interface IEventSerializer
{
    string Serialize(IEvent @event);
    IEvent Deserialize(string eventData, string eventType);
}

public class JsonEventSerializer : IEventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public string Serialize(IEvent @event)
    {
        return JsonSerializer.Serialize(@event, @event.GetType(), Options);
    }

    public IEvent Deserialize(string eventData, string eventType)
    {
        var type = Type.GetType(eventType);
        if (type == null)
            throw new ArgumentException($"Unknown event type: {eventType}");

        var result = JsonSerializer.Deserialize(eventData, type, Options);
        if (result == null)
            throw new InvalidOperationException("Deserialization returned null.");
        return (IEvent)result;
    }
}