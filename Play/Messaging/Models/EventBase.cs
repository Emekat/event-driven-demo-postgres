namespace Play.Messaging.Models;
public abstract class EventBase : IEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string Type => GetType().Name;
    public string AggregateId { get; protected set; } = string.Empty;
    public int Version { get; protected set; }
}