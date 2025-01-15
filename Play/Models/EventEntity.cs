namespace Play.EventStore;

public class EventEntity
{
    public Guid Id { get; set; }
    public string AggregateId { get; set; }
    public string Type { get; set; }
    public string Data { get; set; }
    public int Version { get; set; }
    public DateTime Timestamp { get; set; }
}