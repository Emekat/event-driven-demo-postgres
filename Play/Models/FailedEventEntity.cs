namespace Play.Models;

public class FailedEventEntity
{
    public Guid Id { get; set; }
    public long SequenceNumber { get; set; }
    public string Type { get; set; }
    public string ErrorMessage { get; set; }
    public string StackTrace { get; set; }
    public DateTime FailureTime { get; set; }
    public int RetryCount { get; set; }
    public string Data { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsProcessed { get; set; }
}