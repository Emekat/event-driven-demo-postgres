using Play.EventDispatcher;
using Play.EventStore;
using Play.PostgresEventStore;

namespace Play.Repository;

public class Repository<T>(IEventStore eventStore, IEventDispatcher dispatcher)
    where T : AggregateRoot
{
    public async Task SaveAsync(T aggregate)
    {
        var uncommittedChanges = aggregate.GetUncommittedChanges().ToList();
        await eventStore.SaveEventsAsync(aggregate.Id, uncommittedChanges, 
            aggregate.Version);

        foreach (var @event in uncommittedChanges)
        {
            await dispatcher.DispatchAsync(@event);
        }

        aggregate.MarkChangesAsCommitted();
    }

    public async Task<T> GetByIdAsync(string aggregateId)
    {
        var events = await eventStore.GetEventsAsync(aggregateId);
        var aggregate = Activator.CreateInstance<T>();

        foreach (var @event in events)
        {
            dynamic dynamicAggregate = aggregate;
            dynamicAggregate.Apply(@event);
        }

        return aggregate;
    }
}