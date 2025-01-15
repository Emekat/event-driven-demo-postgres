using Microsoft.EntityFrameworkCore;
using Play.EventStore;

namespace api.Data;

public class EventStoreContext(DbContextOptions<EventStoreContext> options) : DbContext(options)
{
    public DbSet<EventEntity> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventEntity>(builder =>
        {
            builder.ToTable("events");
            
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id");
            
            builder.Property(e => e.AggregateId)
                .HasColumnName("aggregate_id")
                .IsRequired();
            
            builder.Property(e => e.Type)
                .HasColumnName("type")
                .IsRequired();
            
            builder.Property(e => e.Data)
                .HasColumnName("data")
                .HasColumnType("jsonb")  // PostgreSQL specific
                .IsRequired();
            
            builder.Property(e => e.Version)
                .HasColumnName("version")
                .IsRequired();
            
            builder.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();

            // Index for faster lookups
            builder.HasIndex(e => new { e.AggregateId, e.Version })
                .IsUnique();
        });
    }
}