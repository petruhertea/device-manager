using System.Net;
using System.Net.Http.Json;
using DeviceManager.Core.DTOs;
using FluentAssertions;

namespace DeviceManager.IntegrationTests;

[Collection("AuthCollection")]
public class AuthControllerTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly DeviceManagerWebAppFactory _factory;

    private const string AuthEndpoint = "/api/v1/auth";

    public AuthControllerTests(DeviceManagerWebAppFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        _factory.ResetDatabase();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidDto_ReturnsOk_WithUserPayload()
    {
        var dto = new RegisterDto
        {
            FullName = "Dave New",
            Email    = "dave@company.com",
            Password = "Password1!",
            Location = "Dublin"
        };

        var response = await _client.PostAsJsonAsync($"{AuthEndpoint}/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<AuthUserDto>();
        user.Should().NotBeNull();
        user!.Email.Should().Be("dave@company.com");
        user.FullName.Should().Be("Dave New");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // alice@company.com is seeded in the test DB
        var dto = new RegisterDto
        {
            FullName = "Alice Clone",
            Email    = "alice@company.com",
            Password = "Password1!"
        };

        var response = await _client.PostAsJsonAsync($"{AuthEndpoint}/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_MissingFields_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync($"{AuthEndpoint}/register", new { });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk_AndSetsCookie()
    {
        var dto = new LoginDto
        {
            Email    = "alice@company.com",
            Password = "Password1!"
        };

        var response = await _client.PostAsJsonAsync($"{AuthEndpoint}/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // The API sets an HttpOnly cookie — verify the header is present
        response.Headers.TryGetValues("Set-Cookie", out var cookies);
        cookies.Should().NotBeNull();
        cookies!.Any(c => c.StartsWith("auth_token")).Should().BeTrue();

        var user = await response.Content.ReadFromJsonAsync<AuthUserDto>();
        user!.Email.Should().Be("alice@company.com");
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var dto = new LoginDto
        {
            Email    = "alice@company.com",
            Password = "WrongPassword!"
        };

        var response = await _client.PostAsJsonAsync($"{AuthEndpoint}/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        var dto = new LoginDto
        {
            Email    = "nobody@company.com",
            Password = "Password1!"
        };

        var response = await _client.PostAsJsonAsync($"{AuthEndpoint}/login", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_ReturnsNoContent_AndClearsCookie()
    {
        var response = await _client.PostAsync($"{AuthEndpoint}/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Cookie should be cleared (Max-Age=0 or expired)
        response.Headers.TryGetValues("Set-Cookie", out var cookies);
        cookies.Should().NotBeNull();
        cookies!.Any(c => c.Contains("auth_token") && c.Contains("expires=")).Should().BeTrue();
    }

    // ── /me ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Me_WithTestAuthHandler_ReturnsOk()
    {
        // The TestAuthHandler injects a valid principal so /me is reachable
        var response = await _client.GetAsync($"{AuthEndpoint}/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}