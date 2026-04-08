using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeviceManager.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync() || await context.Devices.AnyAsync())
        {
            logger.LogInformation("Database already seeded, skipping.");
            return;
        }

        var scriptPath = Path.Combine(AppContext.BaseDirectory, "scripts", "02_seed_data.sql");
        logger.LogInformation("Looking for seed script at: {Path}", scriptPath);

        if (!File.Exists(scriptPath))
        {
            logger.LogWarning("Seed script not found at: {Path}", scriptPath);
            return;
        }

        logger.LogInformation("Seed script found, running...");

        var sql = await File.ReadAllTextAsync(scriptPath);
        var batches = sql.Split(["\nGO", "\r\nGO"], StringSplitOptions.RemoveEmptyEntries);

        await using var connection = (SqlConnection)context.Database.GetDbConnection();
        await connection.OpenAsync();

        foreach (var batch in batches)
        {
            var trimmed = batch.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            await using var command = new SqlCommand(trimmed, connection);
            await command.ExecuteNonQueryAsync();
        }

        logger.LogInformation("Database seeded successfully.");
    }
}