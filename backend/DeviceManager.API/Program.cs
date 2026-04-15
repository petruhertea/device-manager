using System.Text;
using DeviceManager.Core.Interfaces;
using DeviceManager.Core.Models;
using DeviceManager.Infrastructure.Data;
using DeviceManager.Infrastructure.Repositories;
using DeviceManager.Infrastructure.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase       = false;
    options.User.RequireUniqueEmail         = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
             ?? builder.Configuration["JwtSettings:Key"]
             ?? (builder.Environment.IsEnvironment("Testing")
                 ? "placeholder-key-for-testing-only-not-used"
                 : throw new InvalidOperationException(
                     "JWT_KEY is not set. Add it to your .env file."));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Disable automatic claim type mapping so claim names stay as issued
    // Without this, 'sub' becomes ClaimTypes.NameIdentifier, 'role' becomes ClaimTypes.Role etc.
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["JwtSettings:Issuer"] ?? "DeviceManagerAPI",
        ValidAudience            = builder.Configuration["JwtSettings:Audience"] ?? "DeviceManagerClient",
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["auth_token"];
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CORS — must allow credentials for cookies to work
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();   // required for cookies
    });
});

// Repositories and services
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DatabaseSeeder.SeedAsync(db, userManager,logger);
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// Makes the implicit Program class accessible to the test project
public partial class Program { }