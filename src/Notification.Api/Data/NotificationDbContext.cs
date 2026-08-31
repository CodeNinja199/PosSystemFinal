using Microsoft.EntityFrameworkCore;

using Notification.Api.Entities;

namespace Notification.Api.Data;

// Registered in Program.cs with AddDbContext; the consumer and the controller both use it, each in their own scope.
public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Entities.Notification> Notifications
    {
        get
        {
            return Set<Entities.Notification>();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.Notification>()
            .Property(notification => notification.Type)
            .HasMaxLength(50);

        modelBuilder.Entity<Entities.Notification>()
            .Property(notification => notification.Message)
            .HasMaxLength(500);

        // The notifications page always asks for one user's rows, newest first.
        modelBuilder.Entity<Entities.Notification>()
            .HasIndex(notification => notification.RecipientUserId);
    }
}