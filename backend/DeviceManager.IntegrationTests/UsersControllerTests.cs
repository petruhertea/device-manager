using System.Net;
using System.Net.Http.Json;
using DeviceManager.Core.DTOs;
using FluentAssertions;

namespace DeviceManager.IntegrationTests;

public class UsersControllerTests : IClassFixture<DeviceManagerWebAppFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly DeviceManagerWebAppFactory _factory;

    private const string UserEndpoint = "/api/v1/users";

    public UsersControllerTests(DeviceManagerWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        _factory.ResetDatabase();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // -------------------------------------------------------------------------
    // GET api/v1/users
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAll_ReturnsOk_WithSeededUsers()
    {
        var response = await _client.GetAsync(UserEndpoint);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users!.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetAll_ReturnsUsers_WithAssignedDeviceNames()
    {
        var response = await _client.GetAsync(UserEndpoint);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();

        // Alice has the iPhone assigned to her in seed data
        var alice = users!.First(u => u.Name == "Alice Johnson");
        alice.AssignedDeviceNames.Should().Contain("iPhone 15 Pro");

        // Bob has no devices
        var bob = users.First(u => u.Name == "Bob Smith");
        bob.AssignedDeviceNames.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // GET api/v1/users/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetById_ExistingId_ReturnsCorrectUser()
    {
        var response = await _client.GetAsync(UserEndpoint + "/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Name.Should().Be("Alice Johnson");
        user.Role.Should().Be("Admin");
        user.Location.Should().Be("London");
    }

    [Fact]
    public async Task GetById_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync(UserEndpoint + "/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}