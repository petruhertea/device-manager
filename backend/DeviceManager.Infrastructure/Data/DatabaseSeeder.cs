using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DeviceManager.Core.Models;
using DeviceManager.Infrastructure.Data;

namespace DeviceManager.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

        // Create seed users through Identity so passwords are hashed
        var seedUsers = new[]
        {
            new ApplicationUser
            {
                FullName = "Alice Johnson", Email = "alice@company.com",
                UserName = "alice@company.com", Role = "Admin", Location = "London"
            },
            new ApplicationUser
            {
                FullName = "Bob Smith", Email = "bob@company.com",
                UserName = "bob@company.com", Role = "Employee", Location = "Berlin"
            },
            new ApplicationUser
            {
                FullName = "Carol White", Email = "carol@company.com",
                UserName = "carol@company.com", Role = "Employee", Location = "Paris"
            }
        };

        foreach (var user in seedUsers)
            await userManager.CreateAsync(user, "Password1!");

        var alice = await userManager.FindByEmailAsync("alice@company.com");
        var bob   = await userManager.FindByEmailAsync("bob@company.com");
        var carol = await userManager.FindByEmailAsync("carol@company.com");

        context.Devices.AddRange(
            new Device { Name = "iPhone 15 Pro",  Manufacturer = "Apple",
                         Type = "phone",  OperatingSystem = "iOS",
                         OsVersion = "17.4", Processor = "A17 Pro",
                         RamAmount = 8,  Description = "Apple flagship phone",
                         AssignedUserId = alice!.Id },
            new Device { Name = "Galaxy S24",     Manufacturer = "Samsung",
                         Type = "phone",  OperatingSystem = "Android",
                         OsVersion = "14.0", Processor = "Snapdragon 8 Gen 3",
                         RamAmount = 12, Description = "Samsung flagship phone",
                         AssignedUserId = bob!.Id },
            new Device { Name = "iPad Pro 12.9",  Manufacturer = "Apple",
                         Type = "tablet", OperatingSystem = "iPadOS",
                         OsVersion = "17.4", Processor = "M2",
                         RamAmount = 16, Description = "Apple pro tablet" },
            new Device { Name = "Pixel 8",        Manufacturer = "Google",
                         Type = "phone",  OperatingSystem = "Android",
                         OsVersion = "14.0", Processor = "Tensor G3",
                         RamAmount = 8,  Description = "Google flagship phone",
                         AssignedUserId = carol!.Id },
            new Device { Name = "Galaxy Tab S9",  Manufacturer = "Samsung",
                         Type = "tablet", OperatingSystem = "Android",
                         OsVersion = "14.0", Processor = "Snapdragon 8 Gen 2",
                         RamAmount = 12, Description = "Samsung pro tablet" }
        );

        await context.SaveChangesAsync();
        logger.LogInformation("Database seeded successfully.");
    }
}