using Microsoft.EntityFrameworkCore;
using Play.Models;

namespace Play.Data;

public class EventStoreContext(DbContextOptions<EventStoreContext> options) : DbContext(options)
{
    public DbSet<EventEntity> Events { get; set; }
    public DbSet<FailedEventEntity> FailedEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventEntity>(builder =>
        {
            builder.ToTable("events");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id).HasColumnName("id");
            
            // Add sequence number as a PostgreSQL BIGSERIAL
            builder.Property(e => e.SequenceNumber)
                .HasColumnName("sequence_number")
                .UseIdentityAlwaysColumn()  // This makes it auto-incrementing
                .IsRequired();
            
            builder.Property(e => e.AggregateId)
                .HasColumnName("aggregate_id")
                .IsRequired();
            
            builder.Property(e => e.Type)
                .HasColumnName("type")
                .IsRequired();
            
            builder.Property(e => e.Data)
                .HasColumnName("data")
                .HasColumnType("jsonb") // Store as JSONB
                .IsRequired();
            
            builder.Property(e => e.Version)
                .HasColumnName("version")
                .IsRequired();
            
            builder.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();
            
            // Indexes
            builder.HasIndex(e => e.SequenceNumber)
                .IsUnique();

            // Index for faster lookups
            builder.HasIndex(e => new { e.AggregateId, e.Version })
                .IsUnique();
        });
        
        modelBuilder.Entity<FailedEventEntity>(builder =>
        {
            builder.ToTable("failed_events");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id).HasColumnName("id");
            
            builder.Property(e => e.SequenceNumber)
                .HasColumnName("sequence_number")
                .IsRequired();

            builder.Property(e => e.Type)
                .HasColumnName("type")
                .IsRequired();
            
            builder.Property(e => e.ErrorMessage)
                .HasColumnName("error_message")
                .IsRequired();
            
            builder.Property(e => e.StackTrace)
                .HasColumnName("stack_trace")
                .IsRequired();
            
            builder.Property(e => e.FailureTime)
               .HasColumnName("failure_time")
               .IsRequired();
            
            builder.Property(e => e.RetryCount)
                .HasColumnName("retry_count")
                .IsRequired();
            
            builder.Property(e => e.Data)
                .HasColumnName("data")
                .HasColumnType("jsonb") // Store as JSONB
                .IsRequired();
            
            builder.Property(e => e.Timestamp)
                .HasColumnName("timestamp")
                .IsRequired();
            
            builder.Property(e => e.IsProcessed)
                .HasColumnName("is_processed")
                .IsRequired();
            
            // Indexes
            builder.HasIndex(e => e.SequenceNumber)
                .IsUnique();
        });
    }
}