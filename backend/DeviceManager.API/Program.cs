using DeviceManager.Core.Interfaces;
using DeviceManager.Infrastructure.Data;
using DeviceManager.Infrastructure.Repositories;
using DeviceManager.Infrastructure.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Load .env in development so local runs work without Docker.
// In Docker, these variables are already injected by docker-compose — this is skipped.
// In tests, the Testing environment skips DB registration entirely.
if (builder.Environment.IsDevelopment())
{
    Env.TraversePath().Load();
}

// DbContext — skipped in tests, which register their own in-memory version
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString =
        Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "DATABASE_CONNECTION_STRING is not set. " +
            "Add it to your .env file (local) or docker-compose.yml (Docker).");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString,
            sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
}




// Register repositories
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Register services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();  // built-in, no extra package needed

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// Makes the implicit Program class accessible to the test project
public partial class Program { }