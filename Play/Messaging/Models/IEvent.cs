namespace Play.Messaging.Models;

public interface IEvent
{
    Guid Id { get; }
    DateTime Timestamp { get; }
    string Type { get; }
    string AggregateId { get; }
    int Version { get; }
    long SequenceNumber { get; set; }
    bool IsProcessed { get; set; }
}