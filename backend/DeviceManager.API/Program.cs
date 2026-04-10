using DeviceManager.Core.Interfaces;
using DeviceManager.Infrastructure.Data;
using DeviceManager.Infrastructure.Repositories;
using DeviceManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with SQL Server
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            )
        ));
}

// Register repositories
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Register services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();  // built-in, no extra package needed

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();  // exposes the spec at /openapi/v1.json
    app.MapScalarApiReference();  // UI at /scalar/v1
}


// Seed the database on startup (skip in test environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DatabaseSeeder.SeedAsync(db, logger);
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// Makes the implicit Program class accessible to the test project
public partial class Program { }