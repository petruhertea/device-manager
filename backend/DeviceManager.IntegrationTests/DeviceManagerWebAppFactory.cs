using DeviceManager.Core.Models;
using DeviceManager.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceManager.IntegrationTests;

public class DeviceManagerWebAppFactory : WebApplicationFactory<Program>
{
    private readonly Lock _lock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }

    public void ResetDatabase()
    {
        lock (_lock)
        {
            using var scope = Services.CreateScope();
            var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider
                                   .GetRequiredService<UserManager<ApplicationUser>>();

            db.Devices.RemoveRange(db.Devices);
            // Identity users live in AspNetUsers table
            var users = db.Set<ApplicationUser>().ToList();
            db.Set<ApplicationUser>().RemoveRange(users);
            db.SaveChanges();
            db.ChangeTracker.Clear();

            SeedTestData(db, userManager).GetAwaiter().GetResult();
        }
    }

    private static async Task SeedTestData(
        AppDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        var alice = new ApplicationUser
        {
            Id = 1, FullName = "Alice Johnson", Email = "alice@company.com",
            UserName = "alice@company.com", Role = "Admin", Location = "London"
        };
        var bob = new ApplicationUser
        {
            Id = 2, FullName = "Bob Smith", Email = "bob@company.com",
            UserName = "bob@company.com", Role = "Employee", Location = "Berlin"
        };

        await userManager.CreateAsync(alice, "Password1!");
        await userManager.CreateAsync(bob, "Password1!");

        db.Devices.AddRange(
            new Device { Id = 1, Name = "iPhone 15 Pro", Manufacturer = "Apple",
                         Type = "phone", OperatingSystem = "iOS", OsVersion = "17.4",
                         Processor = "A17 Pro", RamAmount = 8,
                         Description = "Apple flagship", AssignedUserId = alice.Id },
            new Device { Id = 2, Name = "Galaxy S24", Manufacturer = "Samsung",
                         Type = "phone", OperatingSystem = "Android", OsVersion = "14.0",
                         Processor = "Snapdragon 8 Gen 3", RamAmount = 12,
                         Description = "Samsung flagship", AssignedUserId = null }
        );

        db.SaveChanges();
    }
}