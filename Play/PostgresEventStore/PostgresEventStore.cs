using Microsoft.EntityFrameworkCore;
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
            //optimistic concurrency control check
            var currentVersion = await context.Events
                .Where(e => e.AggregateId == aggregateId)
                .MaxAsync(e => (int?)e.Version) ?? -1;

            if (currentVersion != expectedVersion)
                throw new ConcurrencyException($"Expected version {expectedVersion} but got {currentVersion}");

            // Save events
            var eventEntities = events.Select(@event => new EventEntity
            {
                Id = @event.Id,
                Type = @event.Type,
                Data = serializer.Serialize(@event),
                Version = @event.Version,
                Timestamp = @event.Timestamp
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
                    .Where(e => e.SequenceNumber > sequenceNumber)
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
            .Where(e => e.SequenceNumber > sequenceNumber)
            .OrderBy(e => e.SequenceNumber)
            .Take(100) // Batch size - could be configurable
            .ToListAsync();

        return events.Select(e => serializer.Deserialize(e.Data, e.Type));
    }
    
    public async Task<IEnumerable<IEvent>> GetEventsAfterWithAggregateId(string aggregateId, long sequenceNumber)
    {
        var events = await context.Events
            .Where(e => e.AggregateId == aggregateId
                        && e.SequenceNumber > sequenceNumber)
            .OrderBy(e => e.SequenceNumber)
            .Take(100) // Batch size - could be configurable
            .ToListAsync();

        return events.Select(e => serializer.Deserialize(e.Data, e.Type));
    }
    
    public async Task<long> GetLatestSequenceNumber()
    {
        return await context.Events
            .MaxAsync(e => (long?)e.SequenceNumber) ?? 0;
    }

    public async Task SaveFailedEventAsync(IEvent @event, Exception exception)
    {
        var failedEvent = new FailedEventEntity
        {
            Id = Guid.CreateVersion7(),
            SequenceNumber = @event.SequenceNumber,
            Type = @event.GetType().Name,
            ErrorMessage = exception.Message,
            StackTrace = exception.StackTrace,
            FailureTime = DateTime.UtcNow,
            RetryCount = 0,
            Data = serializer.Serialize(@event),
            Timestamp = @event.Timestamp,
            IsProcessed = false
        };
        context.FailedEvents.Add(failedEvent);

        await context.SaveChangesAsync();
    }
}