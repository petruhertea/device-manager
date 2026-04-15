using DeviceManager.Core.Models;
using DeviceManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;

namespace DeviceManager.IntegrationTests;

public class DeviceManagerWebAppFactory : WebApplicationFactory<Program>
{
    private readonly Lock _lock = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["JwtSettings:Key"] = "THIS_IS_A_SUPER_SECRET_KEY_12345",
                ["JwtSettings:Issuer"] = "Test",
                ["JwtSettings:Audience"] = "Test",
                ["JwtSettings:ExpiryMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            // ── Replace SQL Server with in-memory DB ──────────────────────────
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // ── Replace JWT auth with a no-op scheme that auto-authenticates ──
            // Remove every existing authentication registration so there is no
            // conflict with the JwtBearer scheme wired up in Program.cs.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            // Replace the authorization policy so [Authorize] always passes
            services.AddSingleton<IAuthorizationHandler, AllowAnonymousAuthorizationHandler>();
        });
    }

    // ── Database helpers ──────────────────────────────────────────────────────

    public void ResetDatabase()
    {
        lock (_lock)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            db.Devices.RemoveRange(db.Devices);
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
            new Device
            {
                Id = 1, Name = "iPhone 15 Pro", Manufacturer = "Apple",
                Type = "phone", OperatingSystem = "iOS", OsVersion = "17.4",
                Processor = "A17 Pro", RamAmount = 8,
                Description = "Apple flagship", AssignedUserId = alice.Id
            },
            new Device
            {
                Id = 2, Name = "Galaxy S24", Manufacturer = "Samsung",
                Type = "phone", OperatingSystem = "Android", OsVersion = "14.0",
                Processor = "Snapdragon 8 Gen 3", RamAmount = 12,
                Description = "Samsung flagship", AssignedUserId = null
            }
        );

        db.SaveChanges();
    }
}

// ── Test authentication handler ───────────────────────────────────────────────
// Always returns an authenticated principal so [Authorize] never blocks tests.

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "test@company.com"),
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "1"),
            new Claim("role", "Admin"),
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// ── Blanket authorization handler ────────────────────────────────────────────
// Satisfies every IAuthorizationRequirement without inspecting it, so any
// policy-based [Authorize(Policy = "...")] also passes in tests.

public class AllowAnonymousAuthorizationHandler
    : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements.ToList())
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}