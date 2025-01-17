using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Play.Data;
using Play.Exceptions;
using Play.Messaging.Models;
using Play.Models;
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
                Timestamp = @event.Timestamp,
                IsProcessed = @event.IsProcessed
            }).ToList();

            await context.Events.AddRangeAsync(eventEntities);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
        }
    }

    public async Task<IEnumerable<IEvent>> GetEventsAfterAsync(long sequenceNumber, int batchSize, bool replay = false)
    {
        var events = replay? 
                await context.Events
                     .Where(e => e.SequenceNumber > sequenceNumber)
                     .OrderBy(e => e.SequenceNumber)
                     .Take(batchSize)
                    .ToListAsync()
                : 
                await context.Events
                    .Where(e => e.SequenceNumber > sequenceNumber 
                                && !e.IsProcessed)
                    .OrderBy(e => e.SequenceNumber)
                    .Take(batchSize)
                    .ToListAsync();
        
        return events.Select(eventEntity => 
        {
            var domainEvent = serializer.Deserialize(eventEntity.Data, eventEntity.Type);
            domainEvent.SequenceNumber = eventEntity.SequenceNumber;
            return domainEvent;   // This return tells Select what to transform each item into
        });
    }
    
    public async Task<IEnumerable<IEvent>> GetEventsAfter(long sequenceNumber)
    {
        var events = await context.Events
            .Where(e => e.SequenceNumber > sequenceNumber
                        && !e.IsProcessed)
            .OrderBy(e => e.SequenceNumber)
            .Take(100) // Batch size - could be configurable
            .ToListAsync();

        return events.Select(e => serializer.Deserialize(e.Data, e.Type));
    }

    public async Task<long> GetLatestSequenceNumber()
    {
        return await context.Events.Where(e => !e.IsProcessed)
            .MaxAsync(e => (long?)e.SequenceNumber) ?? 0;
    }
    public Task UpdateEventStatusAsync(long sequenceNumber, bool isProcessed)
    {
       var eventEntity = context.Events.FirstOrDefault(e => e.SequenceNumber == sequenceNumber);
       eventEntity!.IsProcessed = isProcessed;
       context.SaveChanges();
       return Task.CompletedTask;       
    }
}