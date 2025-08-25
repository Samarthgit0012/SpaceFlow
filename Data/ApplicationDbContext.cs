using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpaceFlow.Repositories.Models;

namespace SpaceFlow.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Workspace> Workspaces { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Workspace entity
            builder.Entity<Workspace>(entity =>
            {
                entity.HasKey(w => w.Id);
                entity.Property(w => w.Name).IsRequired().HasMaxLength(100);
                entity.Property(w => w.Type).IsRequired().HasMaxLength(50);
                entity.Property(w => w.PricePerHour).HasColumnType("decimal(18,2)");
                entity.Property(w => w.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(w => w.UpdatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(w => w.IsAvailable).HasDefaultValue(true);
                entity.Property(w => w.Amenities).HasMaxLength(500);
                entity.Property(w => w.ImageUrl).HasMaxLength(255);

                // Configure one-to-many relationship with Bookings
                entity.HasMany(w => w.Bookings)
                    .WithOne(b => b.Workspace)
                    .HasForeignKey(b => b.WorkspaceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Booking entity
            builder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(b => b.Status).HasDefaultValue(BookingStatus.Pending);
                entity.Property(b => b.ApplicationUserId).IsRequired();
                entity.Property(b => b.WorkspaceId).IsRequired();

                // Configure relationships
                entity.HasOne(b => b.ApplicationUser)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.ApplicationUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(b => b.Workspace)
                    .WithMany(w => w.Bookings)
                    .HasForeignKey(b => b.WorkspaceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Payment entity
            builder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.PaymentDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.ApplicationUserId).IsRequired();

                // Configure relationships
                entity.HasOne(p => p.ApplicationUser)
                    .WithMany(u => u.Payments)
                    .HasForeignKey(p => p.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Booking)
                    .WithMany()
                    .HasForeignKey(p => p.BookingId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(p => p.Subscription)
                    .WithMany()
                    .HasForeignKey(p => p.SubscriptionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Subscription entity
            builder.Entity<Subscription>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.PlanName).IsRequired().HasMaxLength(100);
                entity.Property(s => s.ApplicationUserId).IsRequired();
                entity.Property(s => s.IsActive).HasDefaultValue(true);

                // Configure relationship
                entity.HasOne(s => s.ApplicationUser)
                    .WithMany(u => u.Subscriptions)
                    .HasForeignKey(s => s.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Notification entity
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
                entity.Property(n => n.ApplicationUserId).IsRequired();
                entity.Property(n => n.IsRead).HasDefaultValue(false);
                entity.Property(n => n.DateCreated).HasDefaultValueSql("GETUTCDATE()");

                // Configure relationship
                entity.HasOne(n => n.ApplicationUser)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ApplicationUser additional properties
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FullName).HasMaxLength(100);
            });

            // Add indexes for better performance
            builder.Entity<Booking>()
                .HasIndex(b => new { b.WorkspaceId, b.StartTime, b.EndTime })
                .HasDatabaseName("IX_Booking_WorkspaceId_StartTime_EndTime");

            builder.Entity<Booking>()
                .HasIndex(b => b.ApplicationUserId)
                .HasDatabaseName("IX_Booking_ApplicationUserId");

            builder.Entity<Payment>()
                .HasIndex(p => p.ApplicationUserId)
                .HasDatabaseName("IX_Payment_ApplicationUserId");

            builder.Entity<Notification>()
                .HasIndex(n => new { n.ApplicationUserId, n.IsRead })
                .HasDatabaseName("IX_Notification_ApplicationUserId_IsRead");
        }
    }
}