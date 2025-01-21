using api;
using api.Contracts;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Play.Data;
using Play.PostgresEventStore;
using Play.Serializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IEventStore, PostgresEventStore>();
builder.Services.AddSingleton<IEventSerializer, JsonEventSerializer>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddDbContext<EventStoreContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("EventStore"), b 
        => b.MigrationsAssembly(typeof(EventStoreContext).Assembly.FullName));
    opt.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddHostedService<EventSubscriber>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure the HTTP request pipeline
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventStoreContext>();
    await context.Database.MigrateAsync();
}

app.Run();