using api;
using api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<EventStoreContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("EventStore")));
builder.Services.AddEventSourcing(builder.Configuration.GetConnectionString("EventStore"));

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventStoreContext>();
    await context.Database.MigrateAsync();
}
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();