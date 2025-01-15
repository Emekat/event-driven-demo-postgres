using api.Data;
using Microsoft.EntityFrameworkCore;
using Play.EventDispatcher;
using Play.EventStore;
using Play.PostgresEventStore;
using Play.Repository;
using Play.Serializer;

namespace api;

public static class EventSourcingServiceCollectionExtensions
{
    public static IServiceCollection AddEventSourcing(
        this IServiceCollection services, 
        string? connectionString)
    {
        // Add DbContext
        services.AddDbContext<EventStoreContext>(options =>
            options.UseNpgsql(connectionString));

        // Add Event Store
        services.AddScoped<IEventStore, PostgresEventStore>();
        services.AddSingleton<IEventSerializer, JsonEventSerializer>();
        services.AddSingleton<IEventDispatcher, EventDispatcher>();

        // Add Generic Repository
        services.AddScoped(typeof(Repository<>));

        return services;
    }
}