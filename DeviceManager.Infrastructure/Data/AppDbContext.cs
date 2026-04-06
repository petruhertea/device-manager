using DeviceManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Device config
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Manufacturer).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Type).IsRequired().HasMaxLength(20);
            entity.Property(d => d.OperatingSystem).IsRequired().HasMaxLength(50);
            entity.Property(d => d.OsVersion).IsRequired().HasMaxLength(50);
            entity.Property(d => d.Processor).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Description).HasMaxLength(500);

            // A device can optionally be assigned to a user
            entity.HasOne(d => d.AssignedUser)
                .WithMany(u => u.AssignedDevices)
                .HasForeignKey(d => d.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // User config
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Location).IsRequired().HasMaxLength(100);
        });
    }
}