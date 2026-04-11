using DeviceManager.Core.Models;
using DeviceManager.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceManager.IntegrationTests;

public class DeviceManagerWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Use a fixed name — we control resets manually via ResetDatabase()
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }

    private readonly Lock _lock = new();

    public void ResetDatabase()
    {
        lock (_lock)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Devices.RemoveRange(db.Devices);
            db.Users.RemoveRange(db.Users);
            db.SaveChanges();

            // Clear the change tracker so EF doesn't think
            // the just-deleted entities are still being tracked
            db.ChangeTracker.Clear();

            SeedTestData(db);
        }
    }

    private static void SeedTestData(AppDbContext db)
    {
        db.Users.AddRange(
            new User { Id = 1, Name = "Alice Johnson", Role = "Admin",    Location = "London" },
            new User { Id = 2, Name = "Bob Smith",     Role = "Employee", Location = "Berlin" }
        );

        db.Devices.AddRange(
            new Device { Id = 1, Name = "iPhone 15 Pro", Manufacturer = "Apple",
                         Type = "phone", OperatingSystem = "iOS", OsVersion = "17.4",
                         Processor = "A17 Pro", RamAmount = 8,
                         Description = "Apple flagship", AssignedUserId = 1 },
            new Device { Id = 2, Name = "Galaxy S24", Manufacturer = "Samsung",
                         Type = "phone", OperatingSystem = "Android", OsVersion = "14.0",
                         Processor = "Snapdragon 8 Gen 3", RamAmount = 12,
                         Description = "Samsung flagship", AssignedUserId = null }
        );

        db.SaveChanges();
    }
}