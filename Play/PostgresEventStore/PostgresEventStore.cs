using api.Data;
using Microsoft.EntityFrameworkCore;
using Play.EventStore;
using Play.Exceptions;
using Play.Messaging.Models;
using Play.Serializer;

namespace Play.PostgresEventStore;

public class PostgresEventStore(EventStoreContext context, IEventSerializer serializer) : IEventStore
{
    public async Task SaveEventsAsync(string aggregateId, IEnumerable<IEvent> events,
        int expectedVersion)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Check version
            var currentVersion = await context.Events
                .Where(e => e.AggregateId == aggregateId)
                .MaxAsync(e => (int?)e.Version) ?? -1;
            
            if (currentVersion != expectedVersion)
                throw new ConcurrencyException($"Expected version {expectedVersion} but got {currentVersion}");

            // Save events
            var eventEntities = events.Select(@event => new EventEntity
            {
                Id = @event.Id,
                AggregateId = @event.AggregateId,
                Type = @event.Type,
                Data = serializer.Serialize(@event),
                Version = @event.Version,
                Timestamp = @event.Timestamp
            }).ToList();

            await context.Events.AddRangeAsync(eventEntities);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<IEvent>> GetEventsAsync(string aggregateId)
    {
        var eventEntities = await context.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToListAsync();

        return eventEntities.Select(e => serializer.Deserialize(e.Data, e.Type));
    }
}