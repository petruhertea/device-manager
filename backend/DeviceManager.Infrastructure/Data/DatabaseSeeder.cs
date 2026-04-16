using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DeviceManager.Core.Models;
using DeviceManager.Infrastructure.Data;

namespace DeviceManager.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync())
        {
            logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        logger.LogInformation("Seeding database...");

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
            },
            new ApplicationUser
            {
                FullName = "Dan Brown", Email = "dan@company.com",
                UserName = "dan@company.com", Role = "Employee", Location = "Amsterdam"
            },
            new ApplicationUser
            {
                FullName = "Eva Green", Email = "eva@company.com",
                UserName = "eva@company.com", Role = "Employee", Location = "Madrid"
            },
        };

        foreach (var user in seedUsers)
            await userManager.CreateAsync(user, "Password1!");

        var alice = await userManager.FindByEmailAsync("alice@company.com");
        var bob   = await userManager.FindByEmailAsync("bob@company.com");
        var carol = await userManager.FindByEmailAsync("carol@company.com");
        var dan   = await userManager.FindByEmailAsync("dan@company.com");
        var eva   = await userManager.FindByEmailAsync("eva@company.com");

        db.Devices.AddRange(
            // ── Apple phones ────────────────────────────────────────────────
            new Device
            {
                Name = "iPhone 15 Pro", Manufacturer = "Apple",
                Type = "phone", OperatingSystem = "iOS", OsVersion = "17.4",
                Processor = "A17 Pro", RamAmount = 8,
                Description = "Apple's flagship phone with a titanium frame and pro camera system.",
                AssignedUserId = alice!.Id
            },
            new Device
            {
                Name = "iPhone 14", Manufacturer = "Apple",
                Type = "phone", OperatingSystem = "iOS", OsVersion = "16.6",
                Processor = "A15 Bionic", RamAmount = 6,
                Description = "Reliable mid-range Apple phone with excellent battery life.",
                AssignedUserId = bob!.Id
            },
            new Device
            {
                Name = "iPhone SE (3rd Gen)", Manufacturer = "Apple",
                Type = "phone", OperatingSystem = "iOS", OsVersion = "17.0",
                Processor = "A15 Bionic", RamAmount = 4,
                Description = "Compact Apple phone with flagship-class performance in a small body.",
                AssignedUserId = null
            },

            // ── Apple tablets ────────────────────────────────────────────────
            new Device
            {
                Name = "iPad Pro 12.9", Manufacturer = "Apple",
                Type = "tablet", OperatingSystem = "iPadOS", OsVersion = "17.4",
                Processor = "M2", RamAmount = 16,
                Description = "Apple's most powerful tablet, suitable for creative professionals.",
                AssignedUserId = null
            },
            new Device
            {
                Name = "iPad Air (5th Gen)", Manufacturer = "Apple",
                Type = "tablet", OperatingSystem = "iPadOS", OsVersion = "17.0",
                Processor = "M1", RamAmount = 8,
                Description = "Versatile Apple tablet balancing performance and portability.",
                AssignedUserId = carol!.Id
            },

            // ── Samsung phones ───────────────────────────────────────────────
            new Device
            {
                Name = "Galaxy S24 Ultra", Manufacturer = "Samsung",
                Type = "phone", OperatingSystem = "Android", OsVersion = "14.0",
                Processor = "Snapdragon 8 Gen 3", RamAmount = 12,
                Description = "Samsung's top-tier phone with S Pen and advanced AI camera features.",
                AssignedUserId = dan!.Id
            },
            new Device
            {
                Name = "Galaxy S24", Manufacturer = "Samsung",
                Type = "phone", OperatingSystem = "Android", OsVersion = "14.0",
                Processor = "Snapdragon 8 Gen 3", RamAmount = 8,
                Description = "Samsung flagship phone with a compact form factor.",
                AssignedUserId = null
            },
            new Device
            {
                Name = "Galaxy A54", Manufacturer = "Samsung",
                Type = "phone", OperatingSystem = "Android", OsVersion = "14.0",
                Processor = "Exynos 1380", RamAmount = 8,
                Description = "Samsung mid-range phone offering solid performance at an accessible price.",
                AssignedUserId = eva!.Id
            },
            new Device
            {
                Name = "Galaxy A14", Manufacturer = "Samsung",
                Type = "phone", OperatingSystem = "Android", OsVersion = "13.0",
                Processor = "Exynos 850", RamAmount = 4,
                Description = "Entry-level Samsung phone designed for everyday tasks.",
                AssignedUserId = null
            },

            // ── Samsung tablets ──────────────────────────────────────────────
            new Device
            {
                Name = "Galaxy Tab S9 Ultra", Manufacturer = "Samsung",
                Type = "tablet", OperatingSystem = "Android", OsVersion = "14.0",
                Processor = "Snapdragon 8 Gen 2", RamAmount = 12,
                Description = "Samsung's largest tablet with an AMOLED display for productivity and media.",
                AssignedUserId = null
            },
            new Device
            {
                Name = "Galaxy Tab S9", Manufacturer = "Samsung",
                Type = "tablet", OperatingSystem = "Android", OsVersion = "14.0",
                Processor = "Snapdragon 8 Gen 2", RamAmount = 8,
                Description = "Well-rounded Samsung tablet for business and entertainment use.",
                AssignedUserId = null
            },

            // ── Google ───────────────────────────────────────────────────────
            new Device
            {
                Name = "Pixel 8 Pro", Manufacturer = "Google",
                Type = "phone", OperatingSystem = "Android", OsVersion = "14.0",
                Processor = "Tensor G3", RamAmount = 12,
                Description = "Google's premium phone with computational photography and on-device AI.",
                AssignedUserId = null
            },
            new Device
            {
                Name = "Pixel 8", Manufacturer = "Google",
                Type = "phone", OperatingSystem = "Android", OsVersion = "14.0",
                Processor = "Tensor G3", RamAmount = 8,
                Description = "Compact Google phone with clean Android and strong camera performance.",
                AssignedUserId = null
            },
            new Device
            {
                Name = "Pixel 7a", Manufacturer = "Google",
                Type = "phone", OperatingSystem = "Android", OsVersion = "13.0",
                Processor = "Tensor G2", RamAmount = 8,
                Description = "Google's mid-range phone delivering a pure Android experience.",
                AssignedUserId = null
            },

            // ── OnePlus ──────────────────────────────────────────────────────
            new Device
            {
                Name = "OnePlus 12", Manufacturer = "OnePlus",
                Type = "phone", OperatingSystem = "Android", OsVersion = "14.0",
                Processor = "Snapdragon 8 Gen 3", RamAmount = 16,
                Description = "High-performance OnePlus phone with fast charging and smooth display.",
                AssignedUserId = null
            }
        );

        await db.SaveChangesAsync();
        logger.LogInformation("Database seeded with {Count} devices.", 15);
    }
}