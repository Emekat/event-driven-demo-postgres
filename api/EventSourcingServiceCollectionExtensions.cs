using Microsoft.EntityFrameworkCore;
using Play.Data;
using Play.PostgresEventStore;
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

        return services;
    }
}