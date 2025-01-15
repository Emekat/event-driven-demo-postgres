using Play.Messaging.Models;

namespace Play.PostgresEventStore;

public interface IEventStore
{
    Task SaveEventsAsync(string aggregateId, IEnumerable<IEvent> events, int expectedVersion);
    Task<IEnumerable<IEvent>> GetEventsAsync(string aggregateId);
}