using Play.Messaging.Models;

namespace Play.PostgresEventStore;

public interface IEventStore
{
    Task SaveEventsAsync(string aggregateId, IEnumerable<IEvent> events, int expectedVersion);
    Task<IEnumerable<IEvent>> GetEventsAfterAsync(long sequenceNumber,  int batchSize, bool replay = false);
    Task<long> GetLatestSequenceNumber();
    Task UpdateEventStatusAsync(long sequenceNumber, bool isProcessed);
}